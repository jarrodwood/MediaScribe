PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE Courses (

  CourseID                 integer PRIMARY KEY AUTOINCREMENT NOT NULL,

  Name                     text NOT NULL,

  LastPlayedTrackID        integer,

  LastPlayedTrackPosition  text,

  EmbeddedVideoWidth       text,

  EmbeddedVideoHeight      text,

  DateCreated              text,

  DateViewed               text

);
CREATE TABLE Hotkeys (

  HotkeyID       integer PRIMARY KEY AUTOINCREMENT NOT NULL,

  Function       integer NOT NULL,

  ModifierKey    integer NOT NULL,

  "Key"          integer NOT NULL,

  Colour         text,

  SeekDirection  integer,

  SeekSeconds    integer,

  Rating         integer

);
INSERT INTO "Hotkeys" VALUES(1,1,2,49,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(2,2,0,74,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(3,3,0,77,'#FF3F3F3F',1,10,0);
INSERT INTO "Hotkeys" VALUES(5,3,0,76,'#FF3F3F3F',0,10,0);
INSERT INTO "Hotkeys" VALUES(6,7,2,45,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(7,7,0,87,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(8,8,2,52,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(9,8,0,85,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(10,9,0,79,'#FFC83232',0,3,0);
INSERT INTO "Hotkeys" VALUES(11,9,0,80,'#FF3232C8',0,3,0);
INSERT INTO "Hotkeys" VALUES(12,9,0,78,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(13,5,0,89,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(14,10,0,81,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(15,11,0,82,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(16,12,0,83,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(17,4,2,35,'#FF3F3F3F',0,3,1);
INSERT INTO "Hotkeys" VALUES(18,4,2,36,'#FF3F3F3F',0,3,2);
INSERT INTO "Hotkeys" VALUES(19,4,2,37,'#FF3F3F3F',0,3,3);
INSERT INTO "Hotkeys" VALUES(21,3,0,75,'#FF3F3F3F',0,3,0);
INSERT INTO "Hotkeys" VALUES(23,6,0,32,'#FF3F3F3F',0,3,0);
CREATE TABLE Notes (

  NoteID            integer PRIMARY KEY AUTOINCREMENT NOT NULL,

  CourseID          integer NOT NULL,

  Body              text NOT NULL,

  StartTrackTimeID  integer,

  EndTrackTimeID    integer,

  Rating            integer,

  BodyInlines       blob,

  BodyStripped      text,

  BodyXaml          text,

  /* Foreign keys */

  FOREIGN KEY (CourseID)

    REFERENCES Courses(CourseID)

    ON DELETE NO ACTION

    ON UPDATE NO ACTION
);
CREATE TABLE Settings (

  SettingID       integer PRIMARY KEY AUTOINCREMENT NOT NULL,

  SerializedData  blob NOT NULL

);
CREATE TABLE TrackTimes (

  TrackTimeID  integer PRIMARY KEY AUTOINCREMENT NOT NULL,

  TrackNumber  integer NOT NULL,

  "Time"       text
);
CREATE TABLE Tracks (

  TrackID      integer PRIMARY KEY AUTOINCREMENT NOT NULL,

  CourseID     integer NOT NULL,

  FilePath     text NOT NULL,

  Title        text,

  Length       text NOT NULL,

  IsVideo      integer NOT NULL,

  AspectRatio  text,

  TrackNumber  integer,

  FileSize     integer,

  /* Foreign keys */

  FOREIGN KEY (CourseID)

    REFERENCES Courses(CourseID)

    ON DELETE NO ACTION

    ON UPDATE NO ACTION

);
DELETE FROM sqlite_sequence;
INSERT INTO "sqlite_sequence" VALUES('Hotkeys',23);
COMMIT;
