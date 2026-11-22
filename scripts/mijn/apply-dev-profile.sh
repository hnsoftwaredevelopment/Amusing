#!/usr/bin/env bash
set -euo pipefail
# Explicitly scoped to the synthetic Mijn development database.
backup="/root/nginx-backups/mijn-profile-db-$(date -u +%Y%m%dT%H%M%SZ).sql"
umask 077
mkdir -p /root/nginx-backups
mariadb-dump amusing_mijn_dev > "$backup"
test -s "$backup"
mysql amusing_mijn_dev < /home/herbert/mijn-dev-deploy/002-profiel.sql
mysql amusing_mijn_dev < /home/herbert/mijn-dev-deploy/dev-profiel-fixtures.sql
printf 'Development database backup: %s\n' "$backup"
