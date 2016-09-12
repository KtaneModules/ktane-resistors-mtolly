.PHONY: check install

check:
	xbuild

install:
	rm -rf "$(HOME)/Library/Application Support/Steam/steamapps/common/Keep Talking and Nobody Explodes/mods/Resistors/"
	cp -R "build/Resistors/" "$(HOME)/Library/Application Support/Steam/steamapps/common/Keep Talking and Nobody Explodes/mods/Resistors"

