FoobarElf-WindowsService
========================

## Required Components

1. Install the foo_httpcontrol component in Foobar (File -> Preferences -> Components -> Install).
2. Copy foobar_httpcontrol_data to `%APPDATA%/foobar2000` directory.
3. Goto [http://localhost/djben](http://127.0.0.1:8888/djben), if you see some information of the music playing in foobar, then you are all set.

## Build

Open solution and go to the build folder. Run cmd as administrator and cd to there, `FoobarElf.exe --install` to install; `FoobarElf.exe --uninstall` to uninstall.

After that, you will see a new system service called `FoobarElf` which starts automatically.
