TapsLite
========

A discreet, lightweight client for Tapestries using SSL and a hardcoded IP.

Allows multiple connections via tabs.

Pages and whispers are highlighted green and blue.  When not the main focus, a sound will ding on pages.  An unselected tab will also color itself green or blue if that connection has received a page or whisper.

Any text surrounded by < > will be highlighted red.  Useful for custom notifications such as <X looked at you>.

When entering text, the bar at the bottom will fill based on buffer size.  If text is sent that is bigger than the buffer, it will automatically be broken up.  If part of a pose or say, it will be automatically spoofed.  If part of a page or whisper, the rest will be paged/whispered automatically to the same recipient(s).
