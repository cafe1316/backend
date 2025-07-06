// src/routes/index.ts
import { Router } from "express";
import { getPopularProductsController, getProductByIDController, searchProductsController, getPaginatedProductsController } from "../controllers/products.controller";

const router = Router();

router.get("/v1/popularProducts", getPopularProductsController);

router.get("/v1/searchProducts", searchProductsController);

router.get("/v1/products", getPaginatedProductsController);

router.get("/v1/products/:id", getProductByIDController);

export default router;