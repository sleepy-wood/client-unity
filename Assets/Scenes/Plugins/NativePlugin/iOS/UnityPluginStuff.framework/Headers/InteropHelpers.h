#ifndef InteropHelpers_h
#define InteropHelpers_h

typedef struct {
    int code;
    char* localizedDescription;
} InteropError;

typedef struct {
    void* pointer;
    int length;
} InteropArray;

#endif
