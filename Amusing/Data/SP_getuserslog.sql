CREATE DEFINER=`root`@`localhost` PROCEDURE `amusing`.`getuserslog`()
BEGIN
	SELECT 
		l.date, 
		l.area, 
		`l`.`action`, 
		l.report 
	FROM amusing.user_log l 
	ORDER BY l.date DESC;
END