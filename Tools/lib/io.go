package lib

import (
	"archive/zip"
	"io"
	"os"
	"path/filepath"
	"strings"
)

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

		zf, err := zp.Create(strings.Replace(relpath, "\\", "/", -1))
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
