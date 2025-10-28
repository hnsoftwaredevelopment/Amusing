CREATE DEFINER=`root`@`localhost` PROCEDURE `amusing`.`getpersonlogins`(IN days_back INT)
BEGIN
	SELECT 
		l.date, 
		l.area, 
		`l`.`action`, 
		l.report 
	FROM amusing.person_log l 
	WHERE l.area="Toegang" 
		AND (days_back IS NULL OR l.date >= DATE_SUB(CURDATE(), INTERVAL days_back DAY))
	ORDER BY l.date DESC;
END