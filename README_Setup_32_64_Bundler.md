# DOF 32/64-bit Setup Bundler

This branch has work I started on a bundler that combines the 32-bit and 64-bit
setups into a single installer application, using the WiX Bundle system.  I
decided to abandon this approach for now (see below), so I checked in the
work on this branch to preserve it in case anyone wants to come back to it
later.

The bundled installer design consists of three separate MSI installs, with a
"bootstrapper" application that provides a typical installer UI and launches
the three sub-MSI installs.  The three sub-installs are:

1. Common files installer, with the various configuration and resource files
that are common to the 32- and 64-bit builds.  This installs the files into
their traditional locations under the main DirectOutput install folder.

2. 32-bit COM/DLL objects.  This is installed in a DOF32 subfolder under the
main DirectOutput install folder.

3. 64-bit COM/DLL objects.  This is installed in a DOF64 subfolder under the
main DirectOutput install folder.

My plan for the bootstrapper application was to provide a common UI to select 
the main DirectOutput install folder location, then run each MSI sub-installer
to install its files into the selected location.

## Why I abandoned this (for now)

After playing with it a little bit, I decided this really doesn't improve the
user experience vs just running the two MSI installers individually.  And it 
has some significant drawbacks.

One big drawback is that the WiX bundler can only be built
as an .EXE, not an .MSI; a lot of people won't like that on security grounds.
(I'm not sure that's a valid reason to prefer MSI, since MSIs can contain
extension code that can do anything an EXE can, but I think it's a widespread
perception that MSIs are at least somewhat safer.)  

A second problem is that the WiX standard bootstrapper UI is pretty ugly.  I
think that doing this properly will require more work writing a whole new 
custom bootstrapper application.  WiX provides a way to do that, but like all
things WiX, it's barely documented.  And it seems rarely done, so it might
even be hard to find examples to work from.

Perhaps the biggest drawback is that the three-piece installer just seems
unnecessarily complicated.  It registers three products under the Windows
uninstall list, which I think most people will find confusing.  I think most
people by now are accustomed to thinking about 32- and 64-bit DOF as separate
installs, but adding a third "common files" product is probably going to
create confusion - lots of users will probably think it's old cruft that
they can delete, and they'll end up hosing their system by removing it.

If I thought the user experience for the install were going to be a lot
better, I'd still go ahead with this despite the drawbacks, but I think
the install will actually be a more pleasant experience just running the
two original MSI files - one for 32-bit, one for 64-bit - by hand.  I think
most users will find this more intuitive, and it makes the separation of
the two parts a lot more transparent.

