-- USE juni_db;



-- Drop tables if they exist (in reverse order of creation to maintain foreign key integrity)
DROP TABLE IF EXISTS product_image;
DROP TABLE IF EXISTS product;
DROP TABLE IF EXISTS product_category;
DROP TABLE IF EXISTS user_profile;
DROP TABLE IF EXISTS user_role;
DROP TABLE IF EXISTS configuration;

DROP TABLE IF EXISTS order_type;
DROP TABLE IF EXISTS order_table;
DROP TABLE IF EXISTS order_details;

-- Create tables
CREATE TABLE user_role (
    role_id INT AUTO_INCREMENT PRIMARY KEY,
    role_title VARCHAR(255) NOT NULL
);


CREATE TABLE user_profile (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NULL,
    surname VARCHAR(255) NULL,
    phone_number VARCHAR(15) NOT NULL,
    password VARCHAR(255) NOT NULL,
    username VARCHAR(50) UNIQUE NULL, -- Added username with unique constraint
    email VARCHAR(255) UNIQUE NULL,    -- Added email with unique constraint
    user_role_id INT NOT NULL DEFAULT 2,
    FOREIGN KEY (user_role_id) REFERENCES user_role(role_id)
);

CREATE TABLE product_category (
    category_id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(255) NOT NULL
);

CREATE TABLE product (
    product_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    price DECIMAL(10, 2) NOT NULL,
    quantity INT NOT NULL,
    category_id INT NOT NULL DEFAULT 1,
    image_count INT NOT NULL DEFAULT 3, -- number of images
    archived BIT NOT NULL DEFAULT 0,-- archive product
    FOREIGN KEY (category_id) REFERENCES product_category(category_id)
);

CREATE TABLE product_image (
    image_id INT AUTO_INCREMENT PRIMARY KEY,
    path VARCHAR(255) NOT NULL,
    product_id INT,
    FOREIGN KEY (product_id) REFERENCES product(product_id)
);

CREATE TABLE configuration
(
  key_name VARCHAR(50) PRIMARY KEY,
  value VARCHAR(50) NOT NULL  
);

CREATE TABLE order_type
(
 id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
 order_type VARCHAR(55) NOT NULL
);

CREATE TABLE order_table(
  id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  customer_id INT NOT NULL REFERENCES user_profile(user_id),
  sender_fullname VARCHAR(30) NOT NULL,
  sender_cell VARCHAR(15) NOT NULL,
  dispatch_address TEXT NULL,
  dest_fullname VARCHAR(30) NULL,
  dest_cell VARCHAR(15) NULL,
  dest_gift_message VARCHAR(255) NULL,
  order_date DATETIME NOT NULL DEFAULT NOW(),-- order current time
  order_type_id INT NOT NULL REFERENCES order_type(id),
  deliveryFee DECIMAL(10,2) NOT NULL DEFAULT 0,
  order_unique_id VARCHAR(20) NOT NULL UNIQUE,
  completed BIT NOT NULL DEFAULT 0  
);

CREATE TABLE order_details(
  order_id INT NOT NULL REFERENCES order_table(id),
  product_id INT NOT NULL REFERENCES product(product_id),
  product_qty INT NOT NULL,
  product_price DECIMAL(10,2) NOT NULL,  
  PRIMARY KEY(order_id,product_id)
);

-- Insert sample data
INSERT INTO configuration(key_name,value) 
VALUES('delivery_fee','10'),('notification_mail','yves.matanga.dev@gmail.com;askjuni@outlook.com');

INSERT INTO user_role (role_title) VALUES ('admin'), ('customer'), ('agent');

INSERT INTO user_profile (name, surname, phone_number, password, username, email, user_role_id) 
VALUES ('Yves', 'Matanga', '0722264804', 'password123', 'yvesm', 'yves.matanga.dev@gmail.com', 1),
       ('Laetitia', 'Kalala', '0816451525', 'kalala123', 'askjuni@outlook.com', 'askjuni@outlook.com', 1),
       ('Bonaventure', 'Kapay', '0816451525', 'kapay123', 'bonakap@gmail.com', 'bonakap@gmail.com', 1);
      
INSERT INTO product_category (title) VALUES ('Autre'), ('Electronique'), ('Habits'), ('Souliers'), ('Sacs');

INSERT INTO order_type(order_type) VALUES
('credit_card_collection'),('credit_card_delivery'),('collection'),('cash_on_delivery');

/*
INSERT INTO product (name, description, price, quantity, category_id,image_count)
VALUES ('Smartphone', 'Latest model with advanced features', 699.99, 50, 1,0),
       ('T-shirt', 'Cotton T-shirt for everyday wear', 19.99, 100, 2,0),
       ('Programming Book', 'Learn programming with this comprehensive guide', 49.99, 30, 3,0);
*/

/*
INSERT INTO product_image (path, product_id)
VALUES ('images/smartphone.jpg', 1),
       ('images/tshirt.jpg', 2),
       ('images/book_cover.jpg', 3);
*/