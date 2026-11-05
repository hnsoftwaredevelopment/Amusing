#!/usr/bin/env bash
set -euo pipefail
umask 077
backup="/root/nginx-backups/mijn-people-db-$(date -u +%Y%m%dT%H%M%SZ).sql"
mkdir -p /root/nginx-backups
mariadb-dump amusing_mijn_dev > "$backup"
test -s "$backup"
mysql amusing_mijn_dev < /home/herbert/mijn-dev-deploy/004-personen.sql
printf 'Development database backup: %s\n' "$backup"
