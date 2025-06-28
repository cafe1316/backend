// src/routes/index.ts
import { Router } from "express";
import { getPopularProductsController, searchDescriptionController, searchProductsController, searchRoastController } from "../controllers/products.controller";

const router = Router();

router.get("/v1/popularProducts", getPopularProductsController);

router.get("/v1/searchProducts", searchProductsController);

router.get("/v1/searchRoast", searchRoastController);

router.get("/v1/searchDescriptiion", searchDescriptionController);

export default router;