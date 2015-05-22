package main

import (
	"bytes"
	"errors"
	"log"
	"os/exec"
	"path/filepath"
	"text/template"

	"golang.org/x/sys/windows/registry"

	"./lib"
)

var bail = lib.Bail

const NSISScript = `Name "{{.ProjectName}}"
Icon "{{.IconFile}}"
Outfile "{{.Outfile}}"
RequestExecutionLevel user

SetCompressor lzma

InstallDir $LOCALAPPDATA\{{.ProjectName}}
Function .onInit
	SetSilent silent
FunctionEnd

Section ""
	MessageBox MB_OK|MB_ICONEXCLAMATION "{{.ProjectName}} test2" /SD IDOK

    SetOutPath "$INSTDIR"
    File /r "{{.InputDirectory}}\*"

    CreateShortCut "$SMPROGRAMS\{{.ProjectName}}.lnk" "$INSTDIR\{{.ProjectName}}.exe" "" "$INSTDIR\{{.ProjectName}}.exe" 0
    Exec "{{.ProjectName}}.exe"
SectionEnd
`

const OutputFileFormat = `{{.Slug}}-{{.BuildDate}}-{{.Version}}.exe`

type NSISData struct {
	ProjectName    string
	IconFile       string
	Outfile        string
	InputDirectory string
}

func RunNSIS(info *lib.Info) (err error) {
	var executable string

	executable, err = exec.LookPath("makensis.exe")
	if err != nil {
		executable = ""
		var nsisPath string
		for _, p := range []string{`SOFTWARE\Wow6432Node\NSIS`, `SOFTWARE\NSIS`} {
			k, err := registry.OpenKey(registry.LOCAL_MACHINE, p, registry.QUERY_VALUE)
			if err != nil {
				continue
			}
			defer k.Close()

			s, _, err := k.GetStringValue("")
			if err != nil {
				continue
			}
			nsisPath = filepath.Clean(s)
			break
		}
		if nsisPath == "" {
			return err
		}

		executable = filepath.Join(nsisPath, "bin/makensis.exe")
	}
	if executable == "" {
		return errors.New("unable to find makensis.exe")
	}

	buf := new(bytes.Buffer)
	err = template.Must(template.New("outputfileformat").Parse(OutputFileFormat)).Execute(buf, info)
	bail(err)

	var data = NSISData{
		ProjectName:    info.Name,
		IconFile:       filepath.Join(info.Path, "WindowsRuntime/helloworld.ico"),
		Outfile:        filepath.Join(info.Path, "Dist/"+buf.String()),
		InputDirectory: filepath.Join(info.Path, "WindowsRuntime/bin/Release"),
	}

	buf = new(bytes.Buffer)
	err = template.Must(template.New("nsisscript").Parse(NSISScript)).Execute(buf, data)
	bail(err)

	cmd := exec.Command(executable, "-")
	cmd.Stdin = buf

	out, err := cmd.CombinedOutput()

	if err != nil {
		log.Println(string(out))
		return err
	}

	info.Result = data.Outfile
	return
}

func main() {

	// Check metadata
	projectpath, _ := filepath.Abs(lib.FindProjectPath())
	info, err := lib.ProjectInfo(projectpath)
	bail(err)
	log.Println("Publiching Windows release:", info)

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

	// Make installer with NSIS:
	log.Println("Creating NSIS installer executable...")
	err = RunNSIS(&info)
	bail(err)
	log.Println("Done: " + info.Result)

}
