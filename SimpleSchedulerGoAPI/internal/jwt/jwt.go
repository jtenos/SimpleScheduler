package jwt

// "time"
import (
	"time"

	"github.com/golang-jwt/jwt"
)

type claims struct {
	Email string `json:"email"`
	jwt.StandardClaims
}

func CreateToken(jwtKey []byte, jwtIssuer string, email string) (tokenStr string, err error) {

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
