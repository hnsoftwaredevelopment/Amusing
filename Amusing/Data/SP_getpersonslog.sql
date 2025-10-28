CREATE DEFINER=`root`@`localhost` PROCEDURE `amusing`.`getpersonslog`()
BEGIN
		SELECT 
		l.date, 
		l.area, 
		`l`.`action`, 
		l.report 
	FROM amusing.person_log l 
	ORDER BY l.date DESC;
END