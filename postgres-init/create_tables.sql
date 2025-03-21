CREATE TABLE inventory (
	barcode BIGINT PRIMARY KEY,
	title TEXT NOT NULL,
	description TEXT,
	quantity INT NOT NULL,
	imageUrl TEXT
);	