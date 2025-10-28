CREATE PROCEDURE amusing.getpayments()
BEGIN
	SELECT 
		l.date, 
		l.area, 
		`l`.`action`, 
		l.report 
	FROM amusing.user_log l 
	WHERE l.area="Finance" 
	ORDER BY l.date DESC;
END