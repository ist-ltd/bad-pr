CREATE DATABASE covid;
USE covid;

CREATE TABLE IF NOT EXISTS areas ( 
	id INT AUTO_INCREMENT PRIMARY KEY,
	areaType VARCHAR(100),
	areaCode VARCHAR(100),
	areaName VARCHAR(500)
);

CREATE TABLE IF NOT EXISTS stat_record (
	id INT AUTO_INCREMENT PRIMARY KEY,
	area_id INT,
	date DATE,
	age_start INT,
	age_end INT,
	cases INT
);