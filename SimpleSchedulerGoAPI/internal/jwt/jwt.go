package jwt

// "time"

// "github.com/golang-jwt/jwt"

func CreateToken(jwtKey []byte, jwtIssuer string, email string) (token string, err error) {
	// TODO: Get this working
	// key, err := jwt.ParseRSAPrivateKeyFromPEM(jwtKey)
	// if err != nil {
	// 	return
	// }
	// t := jwt.New(jwt.SigningMethodRS256)
	// claims := t.Claims.(jwt.MapClaims)
	// claims["exp"] = time.Now().Add(time.Hour * 12)
	// claims["authorized"] = true
	// claims["user"] = email

	// token, err = t.SignedString(key)
	// return
}
