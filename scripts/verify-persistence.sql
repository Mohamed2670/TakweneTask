SET NOCOUNT ON;
IF (SELECT COUNT(*) FROM Artists) < 3 THROW 51000, 'Missing artists', 1;
IF (SELECT COUNT(*) FROM Tracks) < 8 THROW 51000, 'Missing tracks', 1;
IF (SELECT COUNT(*) FROM DSPs) <> 3 THROW 51000, 'Missing DSPs', 1;
IF (SELECT COUNT(*) FROM Users WHERE NormalizedEmail='DEMO@EXAMPLE.COM') <> 1 THROW 51000, 'Demo user seed failed', 1;
IF EXISTS (SELECT 1 FROM Users WHERE LEN(PasswordHash)<60) THROW 51000, 'Invalid password hash', 1;
BEGIN TRANSACTION;
BEGIN TRY
    UPDATE Tracks SET Isrc=(SELECT Isrc FROM Tracks WHERE Id=1) WHERE Id=2;
    THROW 51000, 'Duplicate ISRC accepted', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() NOT IN (2601,2627) BEGIN ROLLBACK; THROW; END;
END CATCH;
ROLLBACK;
BEGIN TRANSACTION;
BEGIN TRY
    UPDATE Tracks SET ArtistId=2147483647 WHERE Id=1;
    THROW 51000, 'Invalid artist FK accepted', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() <> 547 BEGIN ROLLBACK; THROW; END;
END CATCH;
ROLLBACK;
BEGIN TRANSACTION;
BEGIN TRY
    UPDATE Tracks SET Status='invalid' WHERE Id=1;
    THROW 51000, 'Invalid status accepted', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() <> 547 BEGIN ROLLBACK; THROW; END;
END CATCH;
ROLLBACK;
BEGIN TRANSACTION;
BEGIN TRY
    INSERT INTO TrackDistributions (TrackId,DspId,SubmittedAt,Status) SELECT TOP (1) TrackId,DspId,SubmittedAt,Status FROM TrackDistributions;
    THROW 51000, 'Duplicate distribution accepted', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() NOT IN (2601,2627) BEGIN ROLLBACK; THROW; END;
END CATCH;
ROLLBACK;
SELECT (SELECT COUNT(*) FROM Artists) Artists, (SELECT COUNT(*) FROM Tracks) Tracks, (SELECT COUNT(*) FROM DSPs) DSPs, (SELECT COUNT(*) FROM TrackDistributions) Distributions, (SELECT COUNT(*) FROM Users) Users;
PRINT 'Persistence constraint checks passed; test changes rolled back.';
