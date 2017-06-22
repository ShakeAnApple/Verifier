
# tools to use
XBUILD=xbuild
MKBUNDLE=mkbundle --deps --static

# managed projects
MTARGET=Release
MTARGET_PROJECTS_DIR=.
MTARGET_SOLUTION=Verifier.sln
MTARGET_MY_PROJECTS=Verifier PolinaCompiler.Peg
MTARGET_MAIN_PROJECT=Verifier
MTARGETS_MY=Verifier.exe PolinaCompiler.Peg.exe
MTARGET_3RDPARTY_PROJECTS=
MTARGETS_3RDPARTY=

# managed build dirs and targets
MTARGET_PROJECTS=$(MTARGET_MY_PROJECTS) $(MTARGET_3RDPARTY_PROJECTS)
MTARGETS=$(MTARGETS_MY) $(MTARGETS_3RDPARTY)
MTARGET_DIRS=$(MTARGET_PROJECTS:%=$(MTARGET_PROJECTS_DIR)/%/bin) $(MTARGET_PROJECTS:%=$(MTARGET_PROJECTS_DIR)/%/obj)
MTARGETS_DIR=$(MTARGET_PROJECTS_DIR)/$(MTARGET_MAIN_PROJECT)/bin/$(MTARGET)
MTARGET_FILES=$(MTARGETS:%=$(MTARGETS_DIR)/%)

# resulting native executable
NATIVE_TARGET=./verifier

# --------------------------------------------------------------------------------------

.PHONY: all clean

.DEFAULT_GOAL: all

all: main

main:
	$(XBUILD) /p:Configuration=$(MTARGET) $(MTARGET_SOLUTION) && $(MKBUNDLE) -o $(NATIVE_TARGET) $(MTARGET_FILES)

clean: 
	$(XBUILD) /t:Clean && rm -f $(NATIVE_TARGET) && rm -rf $(MTARGET_DIRS)

