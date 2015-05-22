package lib

import (
	"log"
	"os"
	"path/filepath"
	"runtime"
)

func FindProjectPath() string {

	for _, p := range []string{"./", "../", filepath.Join(Here(), "..")} {
		if matches, err := filepath.Glob(filepath.Join(p, "*.sln")); err == nil && len(matches) >= 1 {
			if _, err := os.Stat(matches[0]); err == nil {
				return p
			}
		}
	}
	return ""
}

func Here() string {

	dir := filepath.Dir(os.Args[0])
	_, caller, _, _ := runtime.Caller(1)
	caller = filepath.Dir(caller)
	for _, p := range []string{dir, caller} {
		if _, err := os.Stat(filepath.Join(p, "build-osx.go")); err == nil {
			return p
		}
	}
	return "./"
}

func Bail(err error) bool {
	if err == nil {
		return false
	}

	log.Fatal(err)

	return true
}
