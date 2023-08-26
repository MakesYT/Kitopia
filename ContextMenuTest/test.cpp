#include <ObjectArray.h>

#include "gtest/gtest.h"
#include <nlohmann/json.hpp>

#include <fstream>

using json = nlohmann::json;
TEST(TestCaseName, TestName)
{
    EXPECT_EQ(1, 1);
    EXPECT_TRUE(true);
    const HANDLE eventHandle = CreateEvent(nullptr,FALSE,FALSE, L"Kitopia"); // object name
    std::cout << std::to_string(GetLastError());

    if (GetLastError() == 183)
    {
        MessageBox(nullptr, L"2", L"Kitopia", MB_OK);
        SetEvent(eventHandle);
    }
    else
    {
        MessageBox(nullptr, L"1", L"Kitopia", MB_OK);
    }

    CloseHandle(eventHandle);
}

TEST(d, 1)
{
    SetConsoleOutputCP(CP_UTF8);
    std::ifstream i(
        "D:\\WPF.net\\uToolkitopia\\uToolkitopia\\bin\\Debug\\net7.0-windows10.0.19041.0\\configs\\config.json");
    json j;
    i >> j;

    // 打印JSON内容
    std::cout << j.dump(4, ' ', true) << std::endl;
}
