-- Current sql file was generated after introspecting the database
-- If you want to run this migration please uncomment this code before executing migrations
/*
CREATE TABLE "cafe1316_product_images" (
	"id" serial PRIMARY KEY NOT NULL,
	"product_id" integer,
	"image_url" text NOT NULL,
	"is_primary" boolean DEFAULT false,
	"created_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
	"updated_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);
--> statement-breakpoint
CREATE TABLE "cafe1316_product_categories" (
	"id" serial PRIMARY KEY NOT NULL,
	"name" varchar(100) NOT NULL,
	"parent_id" integer,
	"created_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
	"updated_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);
--> statement-breakpoint
CREATE TABLE "cafe1316_products" (
	"id" serial PRIMARY KEY NOT NULL,
	"sku" varchar(50) NOT NULL,
	"product_name" varchar(255) NOT NULL,
	"origin" varchar(100),
	"description" text,
	"roasting" varchar(50),
	"material" varchar(100),
	"brand" varchar(100),
	"original_price" numeric(10, 2) NOT NULL,
	"discounted_price" numeric(10, 2),
	"unit" varchar(50) NOT NULL,
	"amount" integer NOT NULL,
	"product_type" varchar(50) NOT NULL,
	"category_id" integer,
	"created_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
	"updated_at" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
	"image_url" text,
	"is_hot" boolean DEFAULT false,
	"color" varchar(50),
	"size" varchar(50),
	"weight" numeric(10, 2),
	"specifications" text,
	CONSTRAINT "cafe1316_products_sku_key" UNIQUE("sku")
);
--> statement-breakpoint
ALTER TABLE "cafe1316_product_images" ADD CONSTRAINT "cafe1316_product_images_product_id_fkey" FOREIGN KEY ("product_id") REFERENCES "public"."cafe1316_products"("id") ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE "cafe1316_product_categories" ADD CONSTRAINT "cafe1316_product_categories_parent_id_fkey" FOREIGN KEY ("parent_id") REFERENCES "public"."cafe1316_product_categories"("id") ON DELETE no action ON UPDATE no action;--> statement-breakpoint
ALTER TABLE "cafe1316_products" ADD CONSTRAINT "cafe1316_products_category_id_fkey" FOREIGN KEY ("category_id") REFERENCES "public"."cafe1316_product_categories"("id") ON DELETE no action ON UPDATE no action;
*/