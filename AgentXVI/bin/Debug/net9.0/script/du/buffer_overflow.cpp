#include <iostream>
#include <cstring>

/*Questo serve per testare buffer overflow con input controllato*/

void vulnerableFunction(const char* input) {
    char buffer[16];
    strcpy(buffer, input);  // vulnerabile a overflow se input > 15 char
    std::cout << "Input copiato: " << buffer << std::endl;
}

int main(int argc, char* argv[]) {
    if (argc < 2) {
        std::cout << "Passa un argomento" << std::endl;
        return 1;
    }
    vulnerableFunction(argv[1]);
    return 0;
}
/*Compilare con Visual Studio o g++ per generare eseguibile buffer_overflow.exe
Eseguire con argomento lungo per testare overflow, es: buffer_overflow.exe AAAAAAAAAAAAAAAAAAAAAAAA
Nota: Usare in ambienti controllati per scopi educativi. Non eseguire su sistemi di produzione.*/
