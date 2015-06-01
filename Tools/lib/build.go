package lib

import (
	"io/ioutil"
	"log"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"time"
)

const (
	BuildExecutableUnix    = "xbuild"
	BuildExecutableWindows = "msbuild"

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
	Path    string
	SlnFile string

	Name string
	Slug string

	ShortRev string
	Dirty    bool
	Version  string

	BuildDate string
	BuildYear int

	Result string
}

func ProjectInfo(projectpath string) (info Info, err error) {
	info = Info{
		Path:      projectpath,
		BuildDate: time.Now().UTC().Format("20060102"),
		BuildYear: time.Now().UTC().Year(),
	}

	matches, err := filepath.Glob(filepath.Join(projectpath, "*.sln"))
	if err != nil {
		return
	} else if len(matches) >= 1 {
		info.SlnFile = matches[0]
	}

	projectfile, err := ioutil.ReadFile(filepath.Join(projectpath, ProjectFile))
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

func BuildSolution(info *Info) (err error) {
	var build = BuildExecutableUnix
	if runtime.GOOS == "windows" {
		matches, err := filepath.Glob("c:/Windows/Microsoft.NET/Framework/v4*/MSBuild.exe")
		if err == nil {
			build = matches[len(matches)-1]
		} else {
			build = BuildExecutableWindows
		}
	}

	// Clean Release folder:
	releasepath := filepath.Join(info.Path, "WindowsRuntime/bin/Release")
	os.RemoveAll(releasepath)

	out, err := exec.Command(build, info.SlnFile, "/t:Rebuild", "/p:Configuration=Release").CombinedOutput()
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
