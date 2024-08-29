# WiX V5 Rewrite

This is an aborted start at rewriting the DOF MSI installer for WiX
v4/v5.  I got started on this because I've been working on building a
working installer for 64-bit DOF, which is difficult in WiX v3 because
of a bunch of bugs in the WiX tools when working with 64-bit code.  I
hoped that WiX v4/5 was in better shape for 64-bit products.  What I
found was that WiX v4/5 is not in any shape to use at all, 64-bit
or otherwise.

## Why to move away from WiX v3

As I just mentioned, WiX v3 has some problems dealing with 64-bit
installs.  This isn't going to get better, because WiX v3 is being
aggressively de-supported by the WiX developers.  They REALLY want
everyone to migrate to v4/v5.  In particular, there's a show-stopper
bug in the final v3 release, 3.14.1, which makes the MSI installers it
produces fail, always, in a spectacular way, hosing down your registry
such that you have to find and clean up all of the deliberately
obscured MS Setup keys by hand.  Avoid 3.14.1 at all costs.  This is a
known bug, but the WiX developers have stated that they're never going
to fix it because they want to work on v4/5 instead.  3.14.0 is the
final working v3 release, and the developers don't want you to use
that one, either - or any earlier v3 release - because all earlier v3
releases have two known security vulnerabilities that are considered
significant.  They fixed the security flaws in 3.14.1 and added the
hose-your-registry bug to take its place; all in all, not an equitable
trade.  

For DOF purposes, the security flaws in 3.14.0 DO NOT appear to be a
factor, because they're apparently only triggered by explicitly
invoking certain commands that the DOF MSI doesn't invoke.  So I think
we can safely continue using 3.14.0 despite the dire warnings from the
developers.

## What's on this branch

I took at stab at starting a rewrite for WiX v4/5.  On paper, v4/5
look like they'll be a bit less awful than v3 was - the WiX people
added some new ease-of-use features that replace some of the more
ridiculously bad patterns in v3.  The big gotcha is that v4 is
essentially a whole new language that's large incompatible with v3.
There's a migration tool, but applying it didn't produce a working
install.  After looking at the garbage the migration tool spits out,
and reading some of the v4 documentation, I decided that the easiest
path is to start over from scratch.

This branch captures my aborted start on that v4/5 rewrite.  (I keep
saying "v4/5" because they're apparently basically the same language.
I'm not clear on why they apply the v5 label to v5 at all; maybe they
rewrote all of the tools and kept the language the same or something
like that.)

## Why I gave up on the rewrite

I quickly gave up on the rewrite because v4/5 is for all practical
purposes COMPLETELY UNDOCUMENTED.  There are some Web pages on the WiX
site that purport to be documentation, sure, but they're pretty much
useless.  WiX has always had scant official documentation, but at
least with v3, there was a large community of frustrated users posting
questions on stackoverflow and the like that were many times answered
by other frustrated users who had found workarounds and hacks for what
they were trying to do.  (There are never "solutions"; everything with
WiX is either a workaround or a hack.)  It's a really terrible way to
get information on a programming language, but at least there was a
lot of information out there if you looked hard enough.  It appears
that WiX v4 hasn't been around long enough to develop an equivalent
body of user-to-user help in lieu of documentation, so the lack of
official documentation is all the more apparent.  I quickly decided
that this was going to be way more trouble than it was worth, and went
back to working around the 64-bit issues in v3.  That proved to be
doable with some ugly workarounds.

## Future work

It will probably be necessary to move off of WiX v3 at some point.
There don't appear to be any good non-commercial options out there, so
the only path I can see is to WiX 5 or whatever comes after that.
It's pretty clear that the WiX people aren't ever going to write
usable documentation, but hopefully a user-to-user knowledge base will
develop over time.  It really doesn't look like there's anyone out
there trying to use WiX 4 right now, but maybe that'll change.  Or
maybe some other, non-WiX option will emerge.

