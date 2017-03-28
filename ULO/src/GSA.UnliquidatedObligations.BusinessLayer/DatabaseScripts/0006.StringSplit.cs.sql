CREATE FUNCTION SplitString(@csv_str NVARCHAR(4000), @delimiter nvarchar(20))
 RETURNS @splittable table (val nvarchar(max), pos int)
AS
BEGIN  
 
-- Check for NULL string or empty sting
    IF  (LEN(@csv_str) < 1 OR @csv_str IS NULL)
    BEGIN
        RETURN
    END
 
    ; WITH csvtbl(i,j, pos)
    AS
    (
        SELECT i=1, j= CHARINDEX(@delimiter,@csv_str+@delimiter), 1
 
        UNION ALL 
 
        SELECT i=j+1, j=CHARINDEX(@delimiter,@csv_str+@delimiter,j+1), pos+1
        FROM csvtbl
        WHERE CHARINDEX(@delimiter,@csv_str+@delimiter,j+1) <> 0
    )   
    INSERT  INTO @splittable(val, pos)
    SELECT  SUBSTRING(@csv_str,i,j-i), pos
    FROM    csvtbl 
 
    RETURN
END  

GO
