package main

import (
	"archive/zip"
	"bytes"
	"io"
	"io/ioutil"
	"log"
	"os"
	"os/exec"
	"path"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"text/template"
	"time"
)

const OutputFile = `{{.Name}}-Mac-{{.BuildDate}}-{{.Version}}.zip`

const (
	BuildExecutableUnix    = "xbuild"
	BuildExecutableWindows = "msbuild"
)

const (
	ProjectFile = "WindowsRuntime/WindowsRuntime.csproj"
)

type BuildError struct {
	msg           string
	Output        []byte
	OriginalError error
}

func (e BuildError) Error() string {
	return "Build Error: " + e.msg
}

type Info struct {
	Path      string
	SlnFile   string
	Name      string
	Slug      string
	Version   string
	BuildDate string
	BuildYear int
}

func ProjectInfo(projectpath string) (info Info, err error) {
	info = Info{Path: projectpath}

	matches, err := filepath.Glob(path.Join(projectpath, "*.sln"))
	if err != nil {
		return
	} else if len(matches) >= 1 {
		info.SlnFile = matches[0]
	}

	projectfile, err := ioutil.ReadFile(path.Join(projectpath, ProjectFile))
	if err != nil {
		return
	}
	re := regexp.MustCompile("<AssemblyName>(.*?)</AssemblyName>")
	matches = re.FindStringSubmatch(string(projectfile))
	if len(matches) >= 2 {
		info.Name = matches[1]
		info.Slug = strings.ToLower(info.Name)
	}

	return
}

func GitStatus(prjpath string) (dirty bool, shortrev string, err error) {

	// haxx, for windows gitextension users:
	git := "C:/Program Files (x86)/Git/bin/git.exe"
	_, err = exec.Command(git, "--help").CombinedOutput()
	if err != nil {
		git = "git"
		err = nil
	}

	out, err := exec.Command(git, "--work-tree", prjpath, "status").CombinedOutput()
	if err != nil {
		return
	}
	dirty = !strings.Contains(string(out), "working directory clean")

	out, err = exec.Command(git, "--work-tree", prjpath, "rev-list", "--max-count=1", "--abbrev-commit", "HEAD").CombinedOutput()
	if err != nil {
		return
	}
	shortrev = strings.Trim(string(out), " \r\n")

	return
}

func BuildSolution(slnfile string) (err error) {
	var build = BuildExecutableUnix
	if runtime.GOOS == "windows" {
		matches, err := filepath.Glob("c:/Windows/Microsoft.NET/Framework/v4*/MSBuild.exe")
		if err == nil {
			build = matches[len(matches)-1]
		} else {
			build = BuildExecutableWindows
		}
	}

	out, err := exec.Command(build, slnfile, "/t:Rebuild", "/p:Configuration=Release").CombinedOutput()
	if err != nil {
		log.Println(string(out))
		err = &BuildError{err.Error(), out, err}
		return
	}

	succeeded := strings.Contains(string(out), "Build succeeded.")
	if !succeeded {
		err = &BuildError{"build didn't report success", out, nil}
		return
	}

	return
}

func CopyFile(source string, dest string) (err error) {
	sourcefile, err := os.Open(source)
	if err != nil {
		return err
	}
	defer sourcefile.Close()

	destfile, err := os.Create(dest)
	if err != nil {
		return err
	}
	defer destfile.Close()

	_, err = io.Copy(destfile, sourcefile)
	if err == nil {
		info, err := os.Stat(source)
		err = os.Chmod(dest, info.Mode())
		if err != nil {
			return err
		}
	}
	return
}

func CopyDir(source string, dest string) (err error) {
	info, err := os.Stat(source)
	if err != nil || !info.IsDir() {
		return
	}

	err = os.MkdirAll(dest, info.Mode())
	if err != nil {
		return
	}

	directory, _ := os.Open(source)
	objects, err := directory.Readdir(-1)
	for _, obj := range objects {

		sourcefile := source + "/" + obj.Name()

		destfile := dest + "/" + obj.Name()

		if obj.IsDir() {
			err = CopyDir(sourcefile, destfile)
			if err != nil {
				return
			}
		} else {
			err = CopyFile(sourcefile, destfile)
			if err != nil {
				return
			}
		}
	}
	return
}

func ZipDir(srcdir, destfile string) (err error) {
	fp, err := os.Create(destfile)
	if err != nil {
		return
	}
	zp := zip.NewWriter(fp)

	filepath.Walk(srcdir, func(path string, info os.FileInfo, err error) error {
		relpath, err := filepath.Rel(srcdir, path)
		if info.IsDir() {
			return nil
		}

		zf, err := zp.Create(relpath)
		if err != nil {
			return err
		}

		sf, err := os.Open(path)
		if err != nil {
			return err
		}
		defer sf.Close()

		_, err = io.Copy(zf, sf)
		if err != nil {
			return err
		}
		return nil
	})

	err = zp.Close()
	return
}

func findProjectPath() string {

	for _, p := range []string{"./", "../", path.Join(here(), "..")} {
		if matches, err := filepath.Glob(path.Join(p, "*.sln")); err == nil && len(matches) >= 1 {
			if _, err := os.Stat(matches[0]); err == nil {
				return p
			}
		}
	}
	return ""
}

func here() string {

	dir := filepath.Dir(os.Args[0])
	_, caller, _, _ := runtime.Caller(1)
	caller = filepath.Dir(caller)
	for _, p := range []string{dir, caller} {
		if _, err := os.Stat(path.Join(p, "build-osx.go")); err == nil {
			return p
		}
	}
	return "./"
}

func bail(err error) bool {
	if err == nil {
		return false
	}

	log.Fatal(err)

	return true
}

func main() {

	// Check metadata
	projectpath, _ := filepath.Abs(findProjectPath())
	info, err := ProjectInfo(projectpath)
	bail(err)
	info.BuildDate = time.Now().UTC().Format("20060102")
	info.BuildYear = time.Now().UTC().Year()
	log.Println("Publiching Mac release:", info)

	// Check git, if clean.
	log.Println("Checking GIT status...")
	dirty, shortrev, err := GitStatus(projectpath)
	bail(err)
	if dirty {
		log.Println("WARNING: not a clean git workspace, building development tagged")
		info.Version = "develop"
		log.Println("Done, Building for version:", shortrev, "(develop)")
	} else {
		info.Version = shortrev
		log.Println("Done, Building for version:", info.Version)
	}

	// Run msbuild
	log.Println("Building project in release...")
	err = BuildSolution(info.SlnFile)
	bail(err)
	log.Println("Done.")

	// Compile result:
	log.Println("Combining files into .app directory...")
	dist := path.Join(projectpath, "Dist/")
	os.Mkdir(dist, 0755)
	temp, err := ioutil.TempDir(dist, "Build")
	bail(err)
	defer os.RemoveAll(temp)

	// Plist, icons and mono stuff:
	here := here()
	plistFile := path.Join(temp, info.Name+".app/Contents/Info.plist")
	macDir := path.Join(temp, info.Name+".app/Contents/MacOS")
	resDir := path.Join(temp, info.Name+".app/Contents/Resources")
	os.MkdirAll(macDir, 0755)
	err = os.Mkdir(resDir, 0755)
	bail(err)
	err = CopyDir(path.Join(info.Path, "WindowsRuntime/bin/Release"), macDir)
	bail(err)
	err = CopyDir(path.Join(here, "data/kick"), macDir)
	bail(err)

	plistRaw, err := ioutil.ReadFile(path.Join(here, "data/Info.plist.template"))
	bail(err)
	tmpl := template.Must(template.New("Info.plist").Parse(string(plistRaw)))
	plist, err := os.Create(plistFile)
	bail(err)
	defer plist.Close()
	err = tmpl.Execute(plist, info)
	bail(err)
	os.Chmod(plistFile, 0644)

	matches, err := filepath.Glob(path.Join(projectpath, "Game/Assets/*.icns"))
	bail(err)
	if len(matches) >= 1 {
		err = CopyFile(matches[0], path.Join(resDir, info.Slug+".icns"))
		bail(err)
	}
	log.Println("Done.")

	// Zip result
	log.Println("Compressing into .zip archive...")
	buf := new(bytes.Buffer)
	err = template.Must(template.New("outputfile").Parse(OutputFile)).Execute(buf, info)
	bail(err)
	destfile := path.Join(dist, buf.String())
	err = ZipDir(temp, destfile)
	bail(err)
	log.Println("Done, result:", destfile)

}
