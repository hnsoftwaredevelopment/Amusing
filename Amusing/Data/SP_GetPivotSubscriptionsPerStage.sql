CREATE PROCEDURE amusing.GetPivotSubscriptionsPerStage(IN festival INT)
BEGIN
    DECLARE cols TEXT;
    DECLARE sqlquery TEXT;

    -- prevent GROUP_CONCAT truncation for many distinct podiumsoorten
    SET SESSION group_concat_max_len = 1000000;

    /* Build dynamic SUM(CASE...) expressions for each distinct podiumsoort */
    SELECT GROUP_CONCAT(col_expr ORDER BY podiumsoort SEPARATOR ', ')
    INTO cols
    FROM (
        SELECT 
            podiumsoort,
            CONCAT(
                'SUM(CASE WHEN i.podiumsoort = ',
                QUOTE(podiumsoort),
                ' THEN 1 ELSE 0 END) AS `',
                REPLACE(podiumsoort, '`', '``'),
                '`'
            ) AS col_expr
        FROM (
            SELECT DISTINCT podiumsoort
            FROM Amusing.ah_inschrijvingen
            WHERE festival_id = festival
              AND afgehaakt IS NULL
        ) AS t
    ) AS x;

    /* If no podiumsoorten exist, provide a harmless zero-column */
    IF cols IS NULL OR cols = '' THEN
        SET cols = 'SUM(0) AS `No_Podiumsoorten`';
    END IF;

    /* Build the full SQL (note: use zg.aantal_deelnemers as in your schema) */
    SET sqlquery = CONCAT(
        'SELECT 
            CASE 
                WHEN i.aantal_deelnemers < 10 THEN ''<10''
                WHEN i.aantal_deelnemers >= 10 AND i.aantal_deelnemers < 25 THEN ''>=10''
                WHEN i.aantal_deelnemers >= 25 AND i.aantal_deelnemers < 50 THEN ''>=25''
                WHEN i.aantal_deelnemers >= 50 THEN ''>=50''
            END AS DeelnemersCategorie, ',
            cols, '
        FROM Amusing.ah_inschrijvingen i
        JOIN Amusing.ah_zanggroepen zg 
            ON i.zanggroep_id = zg.zanggroep_id
        WHERE i.festival_id = ', festival, '
          AND i.afgehaakt IS NULL
        GROUP BY DeelnemersCategorie
        ORDER BY FIELD(DeelnemersCategorie, ''<10'',''>=10'',''>=25'',''>=50'');'
    );

    /* Debugging tip: uncomment next line to inspect generated SQL before executing */
    -- SELECT sqlquery;

    /* MySQL PREPARE requires a user/session variable for the SQL text */
    SET @sqlquery := sqlquery;

    PREPARE stmt FROM @sqlquery;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
