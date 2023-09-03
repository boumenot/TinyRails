@echo off
rem This batch file may not work on your machine.  Install csvkit for
rem Python, and add the binaries to your PATH.

set "query=        SELECT"
set "query=%query%   IIF (Name LIKE '%% Car', SUBSTRING(Name, 1, LENGTH(NAME)-4), Name) AS Name,"
rem set "query=%query%   Type,"
set "query=%query%   Quantity       AS Qty,"
set "query=%query%   Include        AS Inc,"
set "query=%query%   Level          AS Lvl,"
rem set "query=%query%   Speed          AS Spd,"
set "query=%query%   Weight         AS Wgt,"
set "query=%query%   Passengers     AS Pas,"
set "query=%query%   Cargo          AS Cgo,"
set "query=%query%   Food           AS Foo,"
set "query=%query%   Comfort        AS Com,"
set "query=%query%   Entertainment  AS Ent,"
set "query=%query%   Facilities     AS Fac,"
set "query=%query%   (Food + Comfort + Entertainment + Facilities) AS Score,"
set "query=%query%   Notes"
set "query=%query% FROM tiny_rails"
set "query=%query% WHERE Cargo < 10 AND Quantity > 0"
set "query=%query% ORDER BY Score DESC, Name ASC, Level ASC"

csvsql --query "%query%" tiny_rails.csv | csvlook
