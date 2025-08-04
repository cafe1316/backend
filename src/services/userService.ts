import db from "../db";
import { cafe1316Users } from "../../drizzle/schema";
import { eq } from "drizzle-orm";
import bcrypt from "bcrypt";
import jwt from "jsonwebtoken";
import {v4 as uuidv4} from 'uuid';

const secret: string = process.env.JWT_SECRET || "";

const id = uuidv4();
console.log(id);

interface RegisterForm {
  username: string;
  password: string;
  confirmPassword: string;
  email: string;
}

interface LoginForm {
  username: string;
  password: string;
}

export const registerNewUser = async (params: RegisterForm):Promise<string> => {
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

  // password needs hashing
  const hashedPassword = await bcrypt.hash(password, 10);
  const generatedUuid = uuidv4();

  // create a new user
  const registerResult = await db.insert(cafe1316Users).values({
    uuid: generatedUuid,
    username, 
    password: hashedPassword, 
    email
  })
  
  // TODO: jwt token
  const token = jwt.sign(
    { userId: generatedUuid, email: email },  // payload
    secret,
    { expiresIn: '1h' }                     // 可选：token 有效期
  );
  return token;

};

export const login = async (params: LoginForm):Promise<any> => {
  if (!params.username || !params.password){
    throw new Error ("username and password are required.");
  }

  const {username, password} = params;
  const res = await db.select().from(cafe1316Users).where(eq(cafe1316Users.username, username))
  if (res.length == 0) {
    throw new Error("user does not exist.");
  }

  let userinfo = res[0];
  if(!bcrypt.compare(password,userinfo.password)) {
    throw new Error("username and password not match.");
  }
  //sign jwt
  const token = jwt.sign(
    { userId: username, email: userinfo.email },  // payload
    secret,
    { expiresIn: '1h' }                     // 可选：token 有效期
  );
  return token;
};
