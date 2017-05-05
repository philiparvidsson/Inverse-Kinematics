#!/usr/bin/make -f

#---------------------------------------
# CONSTANTS
#---------------------------------------

# Game config
BINDIR	 = bin
COMPILER = mcs
FLAGS    = -debug+ -define:DEBUG -define:TRACE -target:winexe
LIBPATHS = $(MONOGAME_PATH)
LIBS     = MonoGame.Framework.dll System.Runtime.Serialization.dll
OBJDIR	 = obj
SRCDIR	 = src
TARGET	 = ik_demo.exe
TMPDIR	 = tmp

#---------------------------------------
# INITIALIZATION
#---------------------------------------

# Linux and macOS have different paths to the MonoGame library files, so make
# sure to set them up properly. No Windows support here, lol!
OS := $(shell uname)

ifeq "$(OS)" "Linux"
MONOGAME_PATH = /usr/lib/mono/xbuild/MonoGame/v3.0
endif

ifeq "$(OS)" "Darwin"
MONOGAME_PATH = /Library/Frameworks/MonoGame.framework/Current
endif

MONOGAME_PATH := $(MONOGAME_PATH)/Assemblies/DesktopGL

#---------------------------------------
# TARGETS
#---------------------------------------

.PHONY: all clean libs run
.PHONY: $(BINDIR)/$(TARGET) compile

all: compile libs

$(BINDIR)/$(TARGET):
	mkdir -p $(BINDIR)
	$(COMPILER) $(FLAGS)                        \
	              $(addprefix -lib:, $(LIBPATHS)) \
	              $(addprefix -r:, $(LIBS))       \
	              -out:$(BINDIR)/$(TARGET)      \
	              -recurse:$(SRCDIR)/*.cs

clean:
	rm -fr $(BINDIR) $(CONTENTFILE) doc

compile: $(BINDIR)/$(TARGET)

doc:
	doxygen

libs:
	mkdir -p $(BINDIR)
	-cp -nr $(MONOGAME_PATH)/* $(BINDIR)

run:
	cd $(BINDIR); \
	mono $(TARGET)
