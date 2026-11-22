#!/usr/bin/env bash
set -euo pipefail
# Invoke with sudo after uploading the archive and these templates to /home/herbert/mijn-dev-deploy.
source_dir=/home/herbert/mijn-dev-deploy
archive="$source_dir/amusing-mijn-dev.tar.gz"
root=/opt/amusing-mijn-dev
stamp=$(date -u +%Y%m%dT%H%M%SZ)
release="$root/releases/$stamp"
backup="/root/nginx-backups/dev-mijn-$stamp"
test -s "$archive"
test -s /etc/amusing-mijn-dev.env
mkdir -p "$release" "$backup"
cp -a /etc/nginx/sites-available/dev.mijn.amusing-hengelo.nl "$backup/nginx.conf"
if test -f /etc/systemd/system/amusing-mijn-dev.service; then
    cp -a /etc/systemd/system/amusing-mijn-dev.service "$backup/service"
fi
if test -L "$root/current"; then readlink -f "$root/current" > "$backup/previous-release"; fi
tar -xzf "$archive" -C "$release"
chown -R root:www-data "$release"
chmod -R u=rwX,g=rX,o= "$release"
chmod 750 "$release/Amusing.Mijn"
install -m 644 "$source_dir/amusing-mijn-dev.service" /etc/systemd/system/amusing-mijn-dev.service
ln -sfn "$release" "$root/current"
systemctl daemon-reload
systemctl enable --now amusing-mijn-dev.service
systemctl restart amusing-mijn-dev.service
ready=0
for attempt in $(seq 1 30); do
    if curl -fsS -H 'Host: dev.mijn.amusing-hengelo.nl' http://127.0.0.1:5002/health >/dev/null; then ready=1; break; fi
    sleep 1
done
test "$ready" = 1
install -m 644 "$source_dir/dev.mijn.nginx.conf" /etc/nginx/sites-available/dev.mijn.amusing-hengelo.nl
if ! nginx -t; then
    cp -a "$backup/nginx.conf" /etc/nginx/sites-available/dev.mijn.amusing-hengelo.nl
    exit 1
fi
systemctl reload nginx
sha256sum "$release/Amusing.Mijn.dll"
systemctl show amusing-mijn-dev.service -p MainPID -p ActiveState -p ActiveEnterTimestamp --no-pager
printf 'Release: %s\nBackup: %s\n' "$release" "$backup"
