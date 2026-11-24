#!/bin/bash
# Copy audio downloaded from 魔王魂 / 効果音ラボ into the game's (git-ignored) audio folder,
# renaming each file to the name the game expects.
#
# Usage:  bash Tools/import_audio.sh [download folder]     (default: ~/Downloads)
# Windows: copy + rename the files by hand using the table in README.md.

SRC="${1:-$HOME/Downloads}"
DST="$(cd "$(dirname "$0")/.." && pwd)/Assets/ThirdParty/Resources/Audio"
mkdir -p "$DST"

# game name            original file name on the site
MAP="
bgm_menu.ogg           maou_loop_bgm_acoustic50.ogg
bgm_game.ogg           maou_loop_bgm_8bit29.ogg
bgm_hurry.ogg          maou_loop_bgm_fantasy15.ogg
sfx_pickup.mp3         washing-machine-laundry-takeout1.mp3
sfx_putdown.mp3        washing-machine-laundry-put1.mp3
sfx_machine_start.mp3  washing-machine-switch1.mp3
sfx_machine_done.mp3   washing-machine-operation-completed1.mp3
sfx_scrub.mp3          bath-wash1.mp3
sfx_iron.mp3           train-steam1.mp3
sfx_stain.mp3          sprinkle-water1.mp3
sfx_fold.mp3           laundry-spread1.mp3
sfx_order.mp3          doorchime1.mp3
sfx_deliver.mp3        clearing1.mp3
sfx_ruined.mp3         incorrect1.mp3
sfx_kid.mp3            thief-boy-guard1.mp3
sfx_kit.mp3            item-get1.mp3
sfx_upgrade.mp3        levelup1.mp3
sfx_click.mp3          decision31.mp3
sfx_timeup.mp3         gong-played2.mp3
"

found=0; missing=0
while read -r name orig; do
  [ -z "$name" ] && continue
  if [ -f "$SRC/$orig" ]; then
    cp "$SRC/$orig" "$DST/$name"; echo "OK       $name  <-  $orig"; found=$((found+1))
  else
    echo "MISSING  $name  (download $orig)"; missing=$((missing+1))
  fi
done <<< "$MAP"
echo "Copied $found file(s), $missing missing. Folder: $DST"
