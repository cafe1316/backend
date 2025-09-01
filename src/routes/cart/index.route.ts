import { Router } from "express";
import {
  addCartController,
  getCartController,
  updateCartController,
  deleteCartController,
} from "../../controllers/cart.controller";
import { authMiddleware } from "../../middleware/auth";

const router = Router();

router.post("/v1/cart/",  addCartController);

router.get("/v1/cart/", authMiddleware, getCartController);

router.put("v1/cart/:id",  updateCartController);

router.delete("v1/cart/:id",  deleteCartController);

export default router;