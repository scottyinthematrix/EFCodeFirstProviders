CREATE FUNCTION ufn_SplitParams (@Array VARCHAR(1000),@separator CHAR(1))
RETURNS @T Table (col1 varchar(50))
AS 
BEGIN
 --DECLARE @T Table (col1 varchar(50))  
 -- @Array is the array we wish to parse
 -- @Separator is the separator charactor such as a comma
 DECLARE @separator_position INT -- This is used to locate each separator character
 DECLARE @array_value VARCHAR(1000) -- this holds each array value as it is returned
 -- For my loop to work I need an extra separator at the end. I always look to the
 -- left of the separator character for each array value

 SET @array = @array + @separator

 -- Loop through the string searching for separtor characters
 WHILE PATINDEX('%' + @separator + '%', @array) <> 0 
 BEGIN
    -- patindex matches the a pattern against a string
    SELECT @separator_position = PATINDEX('%' + @separator + '%',@array)
    SELECT @array_value = LEFT(@array, @separator_position - 1)
    -- This is where you process the values passed.
    INSERT into @T VALUES (@array_value)    
    -- Replace this select statement with your processing
    -- @array_value holds the value of this element of the array
    -- This replaces what we just processed with and empty string
    SELECT @array = STUFF(@array, 1, @separator_position, '')
 END
 RETURN 
END