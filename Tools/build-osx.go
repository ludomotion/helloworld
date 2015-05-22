package main

import (
	"bytes"
	"io/ioutil"
	"log"
	"os"
	"path/filepath"
	"text/template"

	"./lib"
)

var bail = lib.Bail

const OutputFile = `{{.Name}}-Mac-{{.BuildDate}}-{{.Version}}.zip`

func main() {

	// Check metadata
	projectpath, _ := filepath.Abs(lib.FindProjectPath())
	info, err := lib.ProjectInfo(projectpath)
	bail(err)
	log.Println("Publiching Mac release:", info)

	// Check git, if clean.
	log.Println("Checking GIT status...")
	err = lib.GitStatus(projectpath, &info)
	bail(err)
	if info.Dirty {
		log.Println("WARNING: not a clean git workspace, building development tagged")
		log.Println("Done, Building for version:", info.ShortRev, "(develop)")
	} else {
		log.Println("Done, Building for version:", info.Version)
	}

	// Run msbuild
	log.Println("Building project in release...")
	err = lib.BuildSolution(info.SlnFile)
	bail(err)
	log.Println("Done.")

	// Compile result:
	log.Println("Combining files into .app directory...")
	dist := filepath.Join(projectpath, "Dist/")
	os.Mkdir(dist, 0755)
	temp, err := ioutil.TempDir(dist, "Build")
	bail(err)
	defer os.RemoveAll(temp)

	// Plist, icons and mono stuff:
	here := lib.Here()
	plistFile := filepath.Join(temp, info.Name+".app/Contents/Info.plist")
	macDir := filepath.Join(temp, info.Name+".app/Contents/MacOS")
	resDir := filepath.Join(temp, info.Name+".app/Contents/Resources")
	os.MkdirAll(macDir, 0755)
	err = os.Mkdir(resDir, 0755)
	bail(err)
	err = lib.CopyDir(filepath.Join(info.Path, "WindowsRuntime/bin/Release"), macDir)
	bail(err)
	err = lib.CopyDir(filepath.Join(here, "data/kick"), macDir)
	bail(err)

	plistRaw, err := ioutil.ReadFile(filepath.Join(here, "data/Info.plist.template"))
	bail(err)
	tmpl := template.Must(template.New("Info.plist").Parse(string(plistRaw)))
	plist, err := os.Create(plistFile)
	bail(err)
	defer plist.Close()
	err = tmpl.Execute(plist, info)
	bail(err)
	os.Chmod(plistFile, 0644)

	matches, err := filepath.Glob(filepath.Join(projectpath, "Game/Assets/*.icns"))
	bail(err)
	if len(matches) >= 1 {
		err = lib.CopyFile(matches[0], filepath.Join(resDir, info.Slug+".icns"))
		bail(err)
	}
	log.Println("Done.")

	// Zip result
	log.Println("Compressing into .zip archive...")
	buf := new(bytes.Buffer)
	err = template.Must(template.New("outputfile").Parse(OutputFile)).Execute(buf, info)
	bail(err)
	destfile := filepath.Join(dist, buf.String())
	err = lib.ZipDir(temp, destfile)
	bail(err)
	log.Println("Done, result:", destfile)

}
