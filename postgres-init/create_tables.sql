CREATE TABLE inventory (
	barcode TEXT PRIMARY KEY,
	title TEXT NOT NULL,
	description TEXT,
	quantity INT NOT NULL,
	image_path TEXT
);	