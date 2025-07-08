import db from "../db";
import { cafe1316Users } from "../../drizzle/schema";
import { eq } from "drizzle-orm";
import { hash } from "bun";

interface RegisterForm {
  username: string;
  password: string;
  confirmPassword: string;
  email: string;
}

export const registerNewUser = async (params: RegisterForm) => {
  if (
    !params.username ||
    !params.email ||
    !params.password ||
    !params.confirmPassword
  ) {
    // res.json({status: 400, msg: "username or email or password or confirmPassword parameter is missing"})
    throw new Error(
      "username or email or password or confirmPassword parameter is missing"
    );
  }
  // check if password matches with confirmPassword
  const { username, email, password, confirmPassword } = params;
  if (password !== confirmPassword) {
    throw new Error("password does not match!")
  }
  // check if this user already exists
  const res = await db.select().from(cafe1316Users).where(eq(cafe1316Users.username, username))
  if (res.length > 0) {
    throw new Error("user already exists!")
  }
  // create a new user
  const registerResult = await db.insert(cafe1316Users).values({
    username, password, email
  })
  // TODO: password needs hashing
  const bcrypt = require('bcrypt');
  const hashPassword = 'password';
  const saltRounds = 10;
  bcrypt.hash(hashPassword, saltRounds, (err,hash)) => {
    if(err)
  }
  // TODO: jwt token
};
