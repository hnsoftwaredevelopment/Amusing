SET TRANSACTION READ ONLY;
START TRANSACTION WITH CONSISTENT SNAPSHOT;

SELECT JSON_OBJECT('kind','meta','database',DATABASE(),
    'snapshotUtc',DATE_FORMAT(UTC_TIMESTAMP(6),'%Y-%m-%d %H:%i:%s UTC'));

SELECT JSON_OBJECT('kind','role','groupId',r.zanggroep_id,'groupName',g.naam,
    'city',g.standplaats,'groupActive',COALESCE(g.actief,0),
    'role',r.rol,'personId',r.persoon_id,'personExists',p.persoon_id IS NOT NULL,
    'name',CONCAT_WS(' ',NULLIF(TRIM(p.voornaam),''),NULLIF(TRIM(p.tussenvoegsel),''),NULLIF(TRIM(p.achternaam),'')),
    'email',COALESCE(p.email,''),'personActive',COALESCE(p.actief,0),
    'otherRoles',(SELECT GROUP_CONCAT(DISTINCT other.rol ORDER BY other.rol SEPARATOR ', ')
        FROM ah_personen_rollen other WHERE other.zanggroep_id=r.zanggroep_id AND other.persoon_id=r.persoon_id AND other.rol<>r.rol),
    'edition',YEAR(f.festivaldatum),'registered',i.zanggroep_id IS NOT NULL,
    'waitlisted',COALESCE(i.wachtlijst,0),
    'cancelled',i.afgehaakt IS NOT NULL AND CAST(i.afgehaakt AS CHAR) NOT LIKE '0000-%')
FROM ah_personen_rollen r LEFT JOIN ah_zanggroepen g ON g.zanggroep_id=r.zanggroep_id
LEFT JOIN ah_personen p ON p.persoon_id=r.persoon_id
LEFT JOIN ah_festivals f ON f.festival_id=(SELECT festival_id FROM ah_festivals ORDER BY festivaldatum DESC,festival_id DESC LIMIT 1)
LEFT JOIN ah_inschrijvingen i ON i.festival_id=f.festival_id AND i.zanggroep_id=r.zanggroep_id
WHERE r.rol IN ('contactpersoon1','contactpersoon2','penningmeester') AND EXISTS(
    SELECT 1 FROM ah_personen_rollen d WHERE d.zanggroep_id=r.zanggroep_id
        AND d.rol IN ('contactpersoon1','contactpersoon2','penningmeester')
    GROUP BY d.rol HAVING COUNT(DISTINCT d.persoon_id)>1)
ORDER BY r.zanggroep_id,r.rol,r.persoon_id;

SELECT JSON_OBJECT('kind','invitation','groupId',v.group_id,'personId',v.person_id,
    'role',v.role_name,'createdUtc',CAST(v.created_at AS CHAR),'expiresUtc',CAST(v.expires_at AS CHAR))
FROM mijn_role_invitations v WHERE v.accepted_at IS NULL AND v.cancelled_at IS NULL AND v.expires_at>UTC_TIMESTAMP(6)
    AND v.role_name IN ('contactpersoon1','contactpersoon2','penningmeester','overdracht_contactpersoon1') AND EXISTS(
        SELECT 1 FROM ah_personen_rollen d WHERE d.zanggroep_id=v.group_id
            AND d.rol IN ('contactpersoon1','contactpersoon2','penningmeester')
        GROUP BY d.rol HAVING COUNT(DISTINCT d.persoon_id)>1)
ORDER BY v.group_id,v.role_name,v.person_id;

COMMIT;
