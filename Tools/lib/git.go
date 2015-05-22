package lib

import (
	"os/exec"
	"strings"
)

func GitStatus(prjpath string, info *Info) (err error) {

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
	info.Dirty = !strings.Contains(string(out), "working directory clean")

	out, err = exec.Command(git, "--work-tree", prjpath, "rev-list", "--max-count=1", "--abbrev-commit", "HEAD").CombinedOutput()
	if err != nil {
		return
	}
	info.ShortRev = strings.Trim(string(out), " \r\n")
	if info.Dirty {
		info.Version = "develop"
	} else {
		info.Version = info.ShortRev
	}

	return
}
