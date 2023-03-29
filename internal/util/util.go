package util

import (
	"strings"

	"github.com/google/uuid"
)

func UuidLower() string {
	return strings.ReplaceAll(uuid.New().String(), "-", "")
}
