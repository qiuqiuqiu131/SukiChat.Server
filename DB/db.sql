CREATE DATABASE  IF NOT EXISTS `chatserver` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `chatserver`;
-- MySQL dump 10.13  Distrib 8.0.34, for Win64 (x86_64)
--
-- Host: localhost    Database: chatserver
-- ------------------------------------------------------
-- Server version	8.0.35

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20250131110141_init_1','8.0.2'),('20250303150842_group','8.0.2'),('20250304081437_GroupRelation_Add_ByUser','8.0.2'),('20250310104422_userUpdate','8.0.2'),('20250311024350_groupUpdate','8.0.2'),('20250312123326_request_message','8.0.2'),('20250313042835_chatGroupUpdate','8.0.2'),('20250314143534_unreadMessageCount','8.0.2'),('20250315064316_delete','8.0.2'),('20250315064855_delete1','8.0.2'),('20250318064230_groupDeleteUpdate','8.0.2'),('20250318064943_reverteGroupDelete','8.0.2'),('20250318070314_isDisband','8.0.2'),('20250321155722_IsChatting','8.0.2'),('20250323084408_userGroup','8.0.2'),('20250323093109_userGroup_1','8.0.2'),('20250324044806_friendRequestChanged','8.0.2'),('20250326094309_messageUpdate','8.0.2'),('20250326094812_messageUpdate1','8.0.2'),('20250326111750_messageUpdate2','8.0.2'),('20250326115000_dataTimeNullable','8.0.2'),('20250327103524_groupRequestUpdate','8.0.2'),('20250330062423_ChatDetail','8.0.2'),('20250330072851_ChatDetail1','8.0.2'),('20250330104534_ChatMessage_Retract','8.0.2'),('20250405024255_user-detial','8.0.2');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `chatgroupdetails`
--

DROP TABLE IF EXISTS `chatgroupdetails`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `chatgroupdetails` (
  `UserId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ChatGroupId` int NOT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `Time` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  PRIMARY KEY (`UserId`,`ChatGroupId`),
  KEY `IX_ChatGroupDetails_ChatGroupId` (`ChatGroupId`),
  CONSTRAINT `FK_ChatGroupDetails_ChatGroups_ChatGroupId` FOREIGN KEY (`ChatGroupId`) REFERENCES `chatgroups` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ChatGroupDetails_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chatgroupdetails`
--

LOCK TABLES `chatgroupdetails` WRITE;
/*!40000 ALTER TABLE `chatgroupdetails` DISABLE KEYS */;
/*!40000 ALTER TABLE `chatgroupdetails` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `chatgroups`
--

DROP TABLE IF EXISTS `chatgroups`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `chatgroups` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserFromId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `GroupId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Message` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Time` datetime(6) NOT NULL,
  `IsRetracted` tinyint(1) NOT NULL DEFAULT '0',
  `RetractTime` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  PRIMARY KEY (`Id`),
  KEY `IX_ChatGroups_GroupId` (`GroupId`),
  CONSTRAINT `FK_ChatGroups_Groups_GroupId` FOREIGN KEY (`GroupId`) REFERENCES `groups` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chatgroups`
--

LOCK TABLES `chatgroups` WRITE;
/*!40000 ALTER TABLE `chatgroups` DISABLE KEYS */;
/*!40000 ALTER TABLE `chatgroups` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `chatprivatedetails`
--

DROP TABLE IF EXISTS `chatprivatedetails`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `chatprivatedetails` (
  `UserId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ChatPrivateId` int NOT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `Time` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  PRIMARY KEY (`UserId`,`ChatPrivateId`),
  KEY `IX_ChatPrivateDetails_ChatPrivateId` (`ChatPrivateId`),
  CONSTRAINT `FK_ChatPrivateDetails_ChatPrivates_ChatPrivateId` FOREIGN KEY (`ChatPrivateId`) REFERENCES `chatprivates` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ChatPrivateDetails_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chatprivatedetails`
--

LOCK TABLES `chatprivatedetails` WRITE;
/*!40000 ALTER TABLE `chatprivatedetails` DISABLE KEYS */;
/*!40000 ALTER TABLE `chatprivatedetails` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `chatprivates`
--

DROP TABLE IF EXISTS `chatprivates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `chatprivates` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserFromId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UserTargetId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Message` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Time` datetime(6) NOT NULL,
  `IsRetracted` tinyint(1) NOT NULL DEFAULT '0',
  `RetractTime` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  PRIMARY KEY (`Id`),
  KEY `IX_ChatPrivates_UserFromId` (`UserFromId`),
  KEY `IX_ChatPrivates_UserTargetId` (`UserTargetId`),
  CONSTRAINT `FK_ChatPrivates_Users_UserFromId` FOREIGN KEY (`UserFromId`) REFERENCES `users` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ChatPrivates_Users_UserTargetId` FOREIGN KEY (`UserTargetId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chatprivates`
--

LOCK TABLES `chatprivates` WRITE;
/*!40000 ALTER TABLE `chatprivates` DISABLE KEYS */;
/*!40000 ALTER TABLE `chatprivates` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `frienddeletes`
--

DROP TABLE IF EXISTS `frienddeletes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `frienddeletes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId1` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UserId2` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Time` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_FriendDeletes_UserId1` (`UserId1`),
  KEY `IX_FriendDeletes_UserId2` (`UserId2`),
  CONSTRAINT `FK_FriendDeletes_Users_UserId1` FOREIGN KEY (`UserId1`) REFERENCES `users` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_FriendDeletes_Users_UserId2` FOREIGN KEY (`UserId2`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `frienddeletes`
--

LOCK TABLES `frienddeletes` WRITE;
/*!40000 ALTER TABLE `frienddeletes` DISABLE KEYS */;
/*!40000 ALTER TABLE `frienddeletes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `friendrelations`
--

DROP TABLE IF EXISTS `friendrelations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `friendrelations` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `User1Id` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `User2Id` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Grouping` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `GroupTime` datetime(6) NOT NULL,
  `CantDisturb` tinyint(1) NOT NULL DEFAULT '0',
  `IsTop` tinyint(1) NOT NULL DEFAULT '0',
  `Remark` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `LastChatId` int NOT NULL DEFAULT '0',
  `IsChatting` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_FriendRelations_User1Id` (`User1Id`),
  KEY `IX_FriendRelations_User2Id` (`User2Id`),
  CONSTRAINT `FK_FriendRelations_Users_User1Id` FOREIGN KEY (`User1Id`) REFERENCES `users` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_FriendRelations_Users_User2Id` FOREIGN KEY (`User2Id`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `friendrelations`
--

LOCK TABLES `friendrelations` WRITE;
/*!40000 ALTER TABLE `friendrelations` DISABLE KEYS */;
/*!40000 ALTER TABLE `friendrelations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `friendrequests`
--

DROP TABLE IF EXISTS `friendrequests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `friendrequests` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserFromId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UserTargetId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Group` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RequestTime` datetime(6) NOT NULL,
  `IsAccept` tinyint(1) NOT NULL,
  `IsSolved` tinyint(1) NOT NULL,
  `SolveTime` datetime(6) DEFAULT NULL,
  `Message` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Remark` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_FriendRequests_UserFromId` (`UserFromId`),
  KEY `IX_FriendRequests_UserTargetId` (`UserTargetId`),
  CONSTRAINT `FK_FriendRequests_Users_UserFromId` FOREIGN KEY (`UserFromId`) REFERENCES `users` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_FriendRequests_Users_UserTargetId` FOREIGN KEY (`UserTargetId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `friendrequests`
--

LOCK TABLES `friendrequests` WRITE;
/*!40000 ALTER TABLE `friendrequests` DISABLE KEYS */;
/*!40000 ALTER TABLE `friendrequests` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `groupdeletes`
--

DROP TABLE IF EXISTS `groupdeletes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `groupdeletes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `GroupId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `MemberId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DeleteMethod` int NOT NULL,
  `OperateUserId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Time` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_GroupDeletes_MemberId` (`MemberId`),
  KEY `IX_GroupDeletes_OperateUserId` (`OperateUserId`),
  KEY `IX_GroupDeletes_GroupId` (`GroupId`),
  CONSTRAINT `FK_GroupDeletes_Groups_GroupId` FOREIGN KEY (`GroupId`) REFERENCES `groups` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_GroupDeletes_Users_MemberId` FOREIGN KEY (`MemberId`) REFERENCES `users` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_GroupDeletes_Users_OperateUserId` FOREIGN KEY (`OperateUserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `groupdeletes`
--

LOCK TABLES `groupdeletes` WRITE;
/*!40000 ALTER TABLE `groupdeletes` DISABLE KEYS */;
/*!40000 ALTER TABLE `groupdeletes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `grouprelations`
--

DROP TABLE IF EXISTS `grouprelations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `grouprelations` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `GroupId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UserId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Status` int NOT NULL,
  `Grouping` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `JoinTime` datetime(6) NOT NULL,
  `NickName` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Remark` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CantDisturb` tinyint(1) NOT NULL,
  `IsTop` tinyint(1) NOT NULL,
  `LastChatId` int NOT NULL DEFAULT '0',
  `IsChatting` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_GroupRelations_GroupId` (`GroupId`),
  KEY `IX_GroupRelations_UserId` (`UserId`),
  CONSTRAINT `FK_GroupRelations_Groups_GroupId` FOREIGN KEY (`GroupId`) REFERENCES `groups` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_GroupRelations_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `grouprelations`
--

LOCK TABLES `grouprelations` WRITE;
/*!40000 ALTER TABLE `grouprelations` DISABLE KEYS */;
/*!40000 ALTER TABLE `grouprelations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `grouprequests`
--

DROP TABLE IF EXISTS `grouprequests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `grouprequests` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserFromId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `GroupId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RequestTime` datetime(6) NOT NULL,
  `IsAccept` tinyint(1) NOT NULL,
  `IsSolved` tinyint(1) NOT NULL,
  `SolveTime` datetime(6) DEFAULT NULL,
  `AcceptByUserId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Message` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Grouping` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `NickName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Remark` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_GroupRequests_GroupId` (`GroupId`),
  KEY `IX_GroupRequests_UserFromId` (`UserFromId`),
  CONSTRAINT `FK_GroupRequests_Groups_GroupId` FOREIGN KEY (`GroupId`) REFERENCES `groups` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_GroupRequests_Users_UserFromId` FOREIGN KEY (`UserFromId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `grouprequests`
--

LOCK TABLES `grouprequests` WRITE;
/*!40000 ALTER TABLE `grouprequests` DISABLE KEYS */;
/*!40000 ALTER TABLE `grouprequests` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `groups`
--

DROP TABLE IF EXISTS `groups`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `groups` (
  `Id` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CreateTime` datetime(6) NOT NULL,
  `HeadIndex` int NOT NULL DEFAULT '0',
  `IsDisband` tinyint(1) NOT NULL DEFAULT '0',
  `IsCustomHead` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `groups`
--

LOCK TABLES `groups` WRITE;
/*!40000 ALTER TABLE `groups` DISABLE KEYS */;
/*!40000 ALTER TABLE `groups` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sacurityquestions`
--

DROP TABLE IF EXISTS `sacurityquestions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sacurityquestions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Question` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Answer` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UserId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_SacurityQuestions_UserId` (`UserId`),
  CONSTRAINT `FK_SacurityQuestions_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sacurityquestions`
--

LOCK TABLES `sacurityquestions` WRITE;
/*!40000 ALTER TABLE `sacurityquestions` DISABLE KEYS */;
/*!40000 ALTER TABLE `sacurityquestions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usergroups`
--

DROP TABLE IF EXISTS `usergroups`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usergroups` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `GroupName` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `GroupType` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_UserGroups_UserId` (`UserId`),
  CONSTRAINT `FK_UserGroups_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usergroups`
--

LOCK TABLES `usergroups` WRITE;
/*!40000 ALTER TABLE `usergroups` DISABLE KEYS */;
/*!40000 ALTER TABLE `usergroups` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `useronlines`
--

DROP TABLE IF EXISTS `useronlines`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `useronlines` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `LoginTime` datetime(6) NOT NULL,
  `LogoutTime` datetime(6) NOT NULL,
  `UserId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_UserOnlines_UserId` (`UserId`),
  CONSTRAINT `FK_UserOnlines_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `useronlines`
--

LOCK TABLES `useronlines` WRITE;
/*!40000 ALTER TABLE `useronlines` DISABLE KEYS */;
/*!40000 ALTER TABLE `useronlines` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `Id` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsMale` tinyint(1) NOT NULL,
  `Birth` date DEFAULT NULL,
  `Password` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Introduction` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `HeadIndex` int NOT NULL,
  `HeadCount` int NOT NULL,
  `RegisteTime` datetime(6) NOT NULL,
  `LastDeleteFriendMessageTime` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  `LastDeleteGroupMessageTime` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  `LastReadFriendMessageTime` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  `LastReadGroupMessageTime` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  `EmailNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PhoneNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-04-08 19:00:36
