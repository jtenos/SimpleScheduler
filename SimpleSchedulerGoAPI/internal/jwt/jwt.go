package jwt

// "time"
import (
	"fmt"
	"log"
	"time"

	"github.com/golang-jwt/jwt"
)

type EmailClaimKey struct{}
type TokenExpiresKey struct{}

type claims struct {
	Email string `json:"email"`
	jwt.StandardClaims
}

func CreateToken(jwtKey []byte, email string) (tokenStr string, err error) {

	expDt := time.Now().Add(12 * time.Hour)
	claims := &claims{
		Email: email,
		StandardClaims: jwt.StandardClaims{
			ExpiresAt: expDt.Unix(),
		},
	}

	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
	tokenStr, err = token.SignedString(jwtKey)
	return
}

func ReadToken(jwtKey []byte, tokenStr string) (email string, expires time.Time, err error) {
	c := &claims{}
	//log.Println("jwtKey: ", jwtKey)
	//log.Println("tokenStr: ", tokenStr)
	token, err := jwt.ParseWithClaims(tokenStr, c, func(token *jwt.Token) (interface{}, error) {
		return jwtKey, nil
	})

	if err != nil {
		log.Printf("Error parsing token: %s", err.Error())
		if err == jwt.ErrSignatureInvalid {
			err = fmt.Errorf("invalid signature")
			return
		}
		err = fmt.Errorf("unknown error")
		return
	}
	if !token.Valid {
		err = fmt.Errorf("invalid token")
		return
	}

	email = c.Email
	expires = time.Unix(c.ExpiresAt, 0)
	return
}
