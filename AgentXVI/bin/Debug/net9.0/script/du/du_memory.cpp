#include <windows.h>
#include <iostream>
#include <fstream>

/*Compilare con Visual Studio o g++ per generare eseguibile dumpMemory.exe*/

void DumpProcessMemory()
{
    // Opens self process for reading memory
    HANDLE hProcess = GetCurrentProcess();

    MEMORY_BASIC_INFORMATION mbi;
    unsigned char* p = nullptr;
    std::ofstream dump("memory_dump.bin", std::ios::binary);

    while (VirtualQuery(p, &mbi, sizeof(mbi)))
    {
        if (mbi.State == MEM_COMMIT && (mbi.Protect & PAGE_READWRITE))
        {
            unsigned char* buffer = new unsigned char[mbi.RegionSize];
            SIZE_T bytesRead;
            if (ReadProcessMemory(hProcess, p, buffer, mbi.RegionSize, &bytesRead))
            {
                dump.write(reinterpret_cast<char*>(buffer), bytesRead);
            }
            delete[] buffer;
        }
        p += mbi.RegionSize;
    }
    dump.close();
}

int main()
{
    DumpProcessMemory();
    std::cout << "Memory dump completed." << std::endl;
    return 0;
}
