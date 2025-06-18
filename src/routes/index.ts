// src/routes/index.ts
import { Router } from "express";
import { getPopularProductsController, searchProductsController } from "../controllers/products.controller";

const router = Router();

router.get("/v1/popularProducts", getPopularProductsController);

router.get("/v1/searchProducts", searchProductsController);

export default router;