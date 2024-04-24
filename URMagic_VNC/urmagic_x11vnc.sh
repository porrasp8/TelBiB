#!/bin/sh
# 
# file   urmagic_x11vnc.sh
# author Damien LETARD <dle@pygmatec.com>
# modified Yuvarajoo <yuva@deltaglobal.com.my>
# Script d'installation du serveur vnc en service
# 
LOGGER=/usr/bin/logger


log() {
    $LOGGER -p user.info -t "$0[$$]" -- "$1"
}

if [ "$1" = "" ] ; then
    log "no mountpoint supplied, exiting."
    exit 1 ; fi

MOUNTPOINT=$1

log "Point de montage : $MOUNTPOINT" 

# Warn user not to remove USB key
echo "! Installing VNC Server !" | DISPLAY=:0 aosd_cat -R red -x 130 -y -210 -n "Arial Black 40"

for FILE in `find $MOUNTPOINT/package/ -name *.deb`; do
  log "Package trouve : $FILE"
done 

# Install all necessary packages
su -c "dpkg -i $MOUNTPOINT/package/*.deb"

# Create vnc folder
su -c "mkdir /root/.vnc"
# Generate password for VNC connection -- default password "easybot" change following to any personal passwords
su -c "/usr/bin/x11vnc -storepasswd easybot /root/.vnc/passwd"

# Copy vnc scripts
su -c "cp $MOUNTPOINT/script/x11vnc /etc/init.d/"
su -c "chmod u+x /etc/init.d/x11vnc"

# Register service
su -c "update-rc.d x11vnc defaults"

# Start vnc service
su -c "/etc/init.d/x11vnc start"


# Make sure data is written to the USB key
sync
sync

# Notify user it is ok to remove USB key and displays current password
echo "VNC Successfully Installed" | DISPLAY=:0 aosd_cat -x 130 -y -210 -n "Arial Black 40"
echo "Password 'easybot'" | DISPLAY=:0 aosd_cat -x 130 -y -210 -n "Arial Black 40"
