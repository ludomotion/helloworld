package main

import (
	"bufio"
	"fmt"
	"io/ioutil"
	"log"
	"os"
	"os/exec"
	"path"
	"path/filepath"
	"runtime"
	"strings"
)

func prompt(question string) (anwser string) {
	reader := bufio.NewReader(os.Stdin)
	fmt.Print(question)
	anwser, _ = reader.ReadString('\n')
	return
}

func replaceInFile(file string, replacer *strings.Replacer) {
	stat, err := os.Stat(file)
	if err != nil {
		log.Fatalln(err)
	}

	input, err := ioutil.ReadFile(file)
	if err != nil {
		log.Fatalln(err)
	}

	lines := strings.Split(string(input), "\n")

	for i, line := range lines {
		lines[i] = replacer.Replace(line)
	}

	output := strings.Join(lines, "\n")
	err = ioutil.WriteFile(file, []byte(output), stat.Mode())
	if err != nil {
		log.Fatalln(err)
	}
}

func constructReplacer(name string) (replacer *strings.Replacer) {
	name = strings.Title(name)

	title := strings.Split(name, " ")
	lower := strings.Split(strings.ToLower(name), " ")
	upper := strings.Split(strings.ToUpper(name), " ")
	camel := make([]string, len(title))
	copy(camel, title)
	camel[0] = strings.ToLower(camel[0])

	replacer = strings.NewReplacer(
		"helloworld", strings.Join(lower, ""),
		"HelloWorld", strings.Join(title, ""),
		"HELLOWORLD", strings.Join(upper, ""),
		"hello world", strings.Join(lower, " "),
		"Hello World", strings.Join(title, " "),
		"HELLO WORLD", strings.Join(upper, " "),
		"hello_world", strings.Join(lower, "_"),
		"Hello_World", strings.Join(title, "_"),
		"HELLO_WORLD", strings.Join(upper, "_"),
		"helloWorld", strings.Join(camel, ""),
	)
	return
}

func walkProject(projectPath string, replacer *strings.Replacer) (gitdir string) {
	_, caller, _, _ := runtime.Caller(1)
	excludeDir := map[string]bool{
		"bin": true,
		"obj": true,
	}
	excludeExtensions := map[string]bool{
		".suo": true,
	}
	filepath.Walk(projectPath, func(path string, info os.FileInfo, err error) error {
		if info.IsDir() && info.Name() == ".git" {
			if len(gitdir) == 0 {
				gitdir = path
			} else {
				panic("multiple git directories found")
			}
			return filepath.SkipDir
		}
		if info.IsDir() && excludeDir[info.Name()] {
			return filepath.SkipDir
		}
		if info.Name() == "rename-project.go" || info.Name() == "rename-project.exe" || path == caller || path == os.Args[0] {
			return nil
		}
		if !info.IsDir() && !excludeExtensions[filepath.Ext(path)] {
			replaceInFile(path, replacer)
		}
		return nil
	})

	changed := true
	for changed {
		changed = false
		filepath.Walk(projectPath, func(path string, info os.FileInfo, err error) error {
			path, _ = filepath.Abs(path)
			if info.IsDir() && (info.Name() == ".git" || excludeDir[info.Name()]) {
				return filepath.SkipDir
			}
			if info.Name() == "rename-project.go" || info.Name() == "rename-project.exe" || path == caller || path == os.Args[0] {
				return nil
			}
			relpath, _ := filepath.Rel(projectPath, path)
			newpath := filepath.Join(projectPath, replacer.Replace(relpath))
			if path != newpath && !excludeExtensions[filepath.Ext(path)] {
				fmt.Println(path, newpath)
				os.Rename(path, newpath)
				changed = true
			}
			return nil
		})
	}
	return
}

func removeRemote(gitdir string) {
	if len(gitdir) > 0 {
		cmd := exec.Command("git", "--git-dir", gitdir, "remote", "remove", "origin")
		err := cmd.Start()
		if err == nil {
			cmd.Wait()
			fmt.Println("I've removed the git origin remote, add another for your new project")
		} else {
			fmt.Println("Remember to change the url of your git remote")
		}
	}
}

func findProjectPath() string {

	pwd, _ := os.Getwd()
	_, caller, _, _ := runtime.Caller(1)

	var paths []string
	for _, rel := range [...]string{"", "..", "helloworld"} {
		paths = append(paths, path.Join(path.Dir(caller), rel))
		paths = append(paths, path.Join(path.Dir(os.Args[0]), rel))
		paths = append(paths, path.Join(path.Dir(pwd), rel))
	}

	for _, p := range paths {
		if _, err := os.Stat(path.Join(p, "HelloWorld.sln")); err == nil {
			return p
		}
	}
	return ""
}

func main() {
	var projectPath, originalName string
	if len(os.Args) > 1 {
		if os.Args[1] == "-h" {
			fmt.Printf("usage: %s [PROJECTPATH NEWNAME...]\n", path.Base(os.Args[0]))
			return
		}

		projectPath = os.Args[1]
		originalName = strings.Join(os.Args[2:], " ")
	} else {
		projectPath = findProjectPath()
		if len(projectPath) == 0 {
			fmt.Println("Hooo! Not a valid HelloWorld project?")
			return
		}
		originalName = strings.Trim(prompt("Project Name (space seperated, ex. Hello World): "), " \r\n")
	}
	originalName = strings.Title(strings.ToLower(originalName))
	fmt.Println("Renaming Project:", projectPath, " <<< check this path!")
	fmt.Println("Title: Hello World ->", originalName)

	a := prompt("Are you sure? ")
	if strings.ToLower(string(a[0])) != "y" {
		fmt.Println("Not sure, bailing...")
		return
	}

	replacer := constructReplacer(originalName)
	gitdir := walkProject(projectPath, replacer)
	removeRemote(gitdir)
}
