CREATE TABLE inventory (
	barcode TEXT PRIMARY KEY,
	title TEXT NOT NULL,
	description TEXT,
	quantity INT NOT NULL,
	image_path TEXT,
	categories TEXT[],
	updated_at TIMESTAMP NOT NULL
);