# Glossary

Domain terms for RokitIgniter. Use these words in code, tests, tickets and commits.

**Wake tone**: the bundled, inaudible 10Hz WAV (`10Hz.wav`). Playing it keeps KRK Rokit speakers from sleeping after ~30 minutes of silence.
_Avoid_: sound, beep, ping.

**Ignite**: play the wake tone right away and restart the 25-minute cycle. Triggered by a left-click on the tray icon or the Ignite menu item.
_Avoid_: trigger, wake, refresh.

**Cycle**: the repeating schedule that plays the wake tone every 25 minutes (the interval). Exactly one cycle is active while the app runs.

**Igniter**: the platform-neutral scheduler module that owns the cycle. Plays the wake tone on construction, then every interval; restarted by Ignite; stopped by Dispose.
_Avoid_: timer, scheduler service.

**Tone player**: the port the Igniter uses to play the wake tone once (`ITonePlayer`). Knows nothing about scheduling. Adapters: the Windows tone player (`SoundPlayer`) and the Linux tone player (`pw-play` / `paplay` / `aplay`).
_Avoid_: audio service, sound player (except when naming .NET's `SoundPlayer`).

**Single-instance guard**: the module that holds the named mutex `RokitIgniter` so a second launch shows the "already running" notice and exits. The mutex is private to the current user. On Windows it is scoped to the current session, so each Windows session (fast user switching, RDP) runs its own instance. On Linux it is shared across sessions, so a launcher start and a terminal start see each other.

**Composition root**: `Program.cs`. The only place that picks adapters for the current OS and wires them into the Igniter and the tray app.
