#!/usr/bin/env python3

"""
Скрипт для генерации Mermaid диаграммы зависимостей между C# проектами в решении.

НАЗНАЧЕНИЕ:
Этот скрипт анализирует файл решения (.sln) и входящие в него C# проекты (.csproj),
извлекает зависимости между проектами через элементы <ProjectReference>,
и генерирует визуализацию этих зависимостей в виде диаграммы на языке Mermaid.
Результат выводится в стандартный поток вывода и может быть использован в документации,
отчётах или инструментах, поддерживающих Mermaid.

ПРИНЦИП РАБОТЫ:
1. Скрипт ищет единственный файл с расширением `.sln` в указанном каталоге
   — при отсутствии или наличии нескольких `.sln` файлов выдаётся ошибка
2. Парсит `.sln` файл с помощью регулярного выражения, извлекая только `.csproj` проекты
3. Для каждого найденного проекта определяет:
   - является ли он тестовым (наличие сегмента `tests/` в пути, без учёта регистра)
   - путь к `.csproj` файлу (преобразуется в абсолютный)
4. Читает каждый `.csproj` файл как XML и извлекает зависимости из элементов `<ProjectReference>`
5. Сопоставляет имена зависимостей с именами проектов, присутствующих в решении
6. Генерирует диаграмму в формате `graph TD` (top-down), где:
   - каждый проект представлен как узел: `ProjectName["ProjectName"]`
   - каждая зависимость — как направленное ребро: `ПроектA --> ПроектB`
7. При необходимости экранирует специальные символы в именах проектов

КЛЮЧЕВЫЕ ОГРАНИЧЕНИЯ:
- Поддерживаются только проекты в формате `.csproj` (C#); другие типы (C++, F# и т.д.) игнорируются
- Зависимости, указывающие на проекты вне текущего решения, не включаются в диаграмму
- Тестовые проекты исключаются из диаграммы по умолчанию; для их включения требуется флаг `--with-tests`
- Все пути интерпретируются относительно расположения файла `.sln`
- При ошибках разбора XML или отсутствии файлов выбрасываются исключения с сохранением контекста
- Скрипт не модифицирует исходные файлы — работает только в режиме чтения

ИСПОЛЬЗОВАНИЕ:
  python sln-dependency-diagram.py [DIRECTORY] [--with-tests] [--verbose]

ПАРАМЕТРЫ:
  DIRECTORY                     Каталог с решением (по умолчанию: текущий каталог)
  --with-tests                  Включить тестовые проекты (расположенные в подкаталогах `tests/`) в диаграмму
  --verbose                     Включить расширенное логирование (уровень DEBUG)

ПРИМЕРЫ:
  # Сгенерировать диаграмму для решения в текущем каталоге (без тестов)
  python sln-dependency-diagram.py

  # Сгенерировать диаграмму с учётом тестовых проектов
  python sln-dependency-diagram.py --with-tests

  # Запустить с подробным логированием для отладки
  python sln-dependency-diagram.py --verbose

  # Проанализировать решение в указанном каталоге
  python sln-dependency-diagram.py ./src/my-solution
"""

import argparse
import logging
import re
import sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass, field
from pathlib import Path
from typing import Dict, List, Set, Pattern
from collections import defaultdict


@dataclass
class Project:
    """Класс для представления C# проекта"""
    name: str
    csproj_path: Path
    relative_path: str
    is_test: bool = field(default=False, init=False)
    dependencies: Set[str] = field(default_factory=set, init=False)
    
    def __post_init__(self) -> None:
        """Определяем, является ли проект тестовым"""
        # Проверяем наличие 'tests/' в пути (регистронезависимо)
        self.is_test = any(
            part.lower() == 'tests' 
            for part in self.csproj_path.parts
        )


@dataclass
class Solution:
    """Класс для представления решения .sln"""
    path: Path
    directory: Path
    projects: Dict[str, Project] = field(default_factory=dict)
    
    @property
    def name(self) -> str:
        """Имя решения (без расширения)"""
        return self.path.stem


class SolutionParser:
    """Парсер файла решения .sln"""
    
    # Регулярное выражение для извлечения проектов из .sln
    # Формат: Project("{GUID}") = "ProjectName", "RelativePath\Project.csproj", "{GUID}"
    PROJECT_PATTERN: Pattern[str] = re.compile(
        r'Project\("\{[^}]+\}"\)\s*=\s*"([^"]+)"\s*,\s*"([^"]+\.csproj)"\s*,\s*"\{[^}]+\}"',
        re.IGNORECASE
    )
    
    @classmethod
    def parse(cls, sln_path: Path) -> Solution:
        """Парсит файл .sln и возвращает объект Solution"""
        logging.debug(f"Parsing solution file: {sln_path}")
        
        if not sln_path.exists():
            raise FileNotFoundError(f"Solution file not found: {sln_path}")
        
        solution = Solution(path=sln_path, directory=sln_path.parent)
        
        try:
            content = sln_path.read_text(encoding='utf-8-sig')
        except UnicodeDecodeError as e:
            raise ValueError(f"Cannot read solution file (invalid encoding): {sln_path}") from e
        
        for line in content.splitlines():
            match = cls.PROJECT_PATTERN.search(line)
            if match:
                project_name = match.group(1)
                relative_path = match.group(2)
                
                # Пропускаем не-C# проекты (двойная проверка через расширение)
                if not relative_path.lower().endswith('.csproj'):
                    logging.debug(f"Skipping non-C# project: {project_name}")
                    continue
                
                # Преобразуем относительный путь в абсолютный
                # Заменяем обратные слеши на прямые для кроссплатформенности
                normalized_path = relative_path.replace('\\', '/')
                csproj_path = (sln_path.parent / normalized_path).resolve()
                
                if not csproj_path.exists():
                    raise ValueError(f"Project file not found: {csproj_path}")
                
                project = Project(
                    name=project_name,
                    csproj_path=csproj_path,
                    relative_path=relative_path
                )
                
                solution.projects[project_name] = project
                logging.debug(f"Found project: {project_name} ({csproj_path})")
        
        if not solution.projects:
            raise ValueError(f"No C# projects found in solution: {sln_path}")
        
        logging.debug(f"Found {len(solution.projects)} projects in solution")
        return solution


class CsprojParser:
    """Парсер файлов .csproj для извлечения зависимостей между проектами"""
    
    @classmethod
    def parse_dependencies(cls, csproj_path: Path) -> Set[str]:
        """Извлекает зависимости на другие проекты из файла .csproj"""
        logging.debug(f"Parsing project file: {csproj_path}")
        
        if not csproj_path.exists():
            raise FileNotFoundError(f"Project file not found: {csproj_path}")
        
        try:
            tree = ET.parse(csproj_path)
        except ET.ParseError as e:
            raise ValueError(f"Invalid XML in project file: {csproj_path}") from e
        
        root = tree.getroot()
        
        # Обрабатываем пространства имён
        namespaces = {'ns': root.tag.split('}')[0].strip('{')} if '}' in root.tag else {}
        
        dependencies: Set[str] = set()
        
        # Ищем все ProjectReference элементы
        # Используем XPath с учётом пространства имён, если оно есть
        if namespaces:
            ref_elements = root.findall('.//ns:ProjectReference', namespaces)
        else:
            # Ищем без пространства имён
            ref_elements = []
            for elem in root.iter():
                if 'ProjectReference' in elem.tag:
                    ref_elements.append(elem)
        
        for ref in ref_elements:
            include_attr = ref.get('Include')
            if include_attr:
                # Извлекаем имя проекта из пути
                # Формат: ..\OtherProject\OtherProject.csproj или ../OtherProject/OtherProject.csproj
                try:
                    # Преобразуем в Path и получаем имя файла без расширения
                    ref_path = Path(include_attr.replace('\\', '/'))
                    project_name = ref_path.stem
                    dependencies.add(project_name)
                    logging.debug(f"  Found dependency: {project_name}")
                except Exception as e:
                    raise RuntimeError(f"  Invalid project reference format: {include_attr}") from e
        
        return dependencies


class DependencyAnalyzer:
    """Анализатор зависимостей между проектами"""
    
    @classmethod
    def analyze(cls, solution: Solution, include_tests: bool = False) -> Dict[str, List[str]]:
        """Анализирует зависимости между проектами в решении"""
        logging.debug("Analyzing project dependencies...")
        
        # Собираем зависимости для каждого проекта
        dependencies: Dict[str, List[str]] = defaultdict(list)
        
        for project_name, project in solution.projects.items():
            # Пропускаем тестовые проекты, если не включен флаг
            if project.is_test and not include_tests:
                logging.debug(f"Skipping test project: {project_name}")
                continue
            
            try:
                deps = CsprojParser.parse_dependencies(project.csproj_path)
                
                # Фильтруем зависимости, оставляя только проекты из текущего решения
                for dep in deps:
                    if dep in solution.projects:
                        dependencies[project_name].append(dep)
                    else:
                        logging.debug(f"Dependency on external project: {dep} (not in solution)")
            
            except Exception as e:
                logging.exception(f"Failed to parse dependencies for project: {project_name}")
                # Продолжаем анализ для других проектов
        
        return dict(dependencies)


class MermaidGenerator:
    """Генератор диаграмм в формате Mermaid"""
    
    @classmethod
    def escape_name(cls, name: str) -> str:
        """Экранирует имя проекта для Mermaid диаграммы"""
        # Заменяем символы, которые могут быть проблематичными в Mermaid
        return name.replace('"', '\\"').replace("'", "\\'")
    
    @classmethod
    def generate_diagram(cls, dependencies: Dict[str, List[str]], solution_name: str) -> str:
        """Генерирует Mermaid диаграмму зависимостей"""
        lines = [
            f"%% Dependencies diagram for solution: {solution_name}",
            "%% Generated by print-modules-graph",
            "",
            "graph TD"
        ]
        
        # Собираем все уникальные проекты
        all_projects = set(dependencies.keys())
        for deps in dependencies.values():
            all_projects.update(deps)
        
        # Добавляем узлы (проекты)
        for project in sorted(all_projects):
            escaped_name = cls.escape_name(project)
            lines.append(f'    {project}["{escaped_name}"]')
        
        lines.append("")  # Пустая строка для разделения
        
        # Добавляем зависимости (стрелки)
        for source, targets in sorted(dependencies.items()):
            for target in sorted(targets):
                lines.append(f"    {source} --> {target}")
        
        return "\n".join(lines)


def find_solution_file(directory: Path) -> Path:
    """Находит файл .sln в указанном каталоге"""
    sln_files = list(directory.glob("*.sln"))
    
    if not sln_files:
        raise FileNotFoundError(f"No .sln files found in directory: {directory}")
    
    if len(sln_files) > 1:
        raise ValueError(f"Multiple .sln files found in directory: {directory}. Please specify which one to use.")
    
    return sln_files[0]


def setup_logging(verbose: bool) -> None:
    """Настраивает логирование"""
    level = logging.DEBUG if verbose else logging.INFO
    logging.basicConfig(
        level=level,
        format='%(levelname)s: %(message)s',
        handlers=[logging.StreamHandler(sys.stderr)]
    )


def main() -> None:
    """Основная функция скрипта"""
    parser = argparse.ArgumentParser(
        description="Generate Mermaid diagram of dependencies between C# projects in a .sln solution"
    )
    parser.add_argument(
        "directory",
        nargs="?",
        default=".",
        help="Project directory (default: current directory)"
    )
    parser.add_argument(
        "--with-tests",
        action="store_true",
        help="Include test projects (located in tests/)"
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Enable verbose logging"
    )
    
    args = parser.parse_args()
    
    # Настраиваем логирование
    setup_logging(args.verbose)
    
    try:
        # Преобразуем путь в абсолютный
        directory = Path(args.directory).resolve()
        logging.debug(f"Analyzing directory: {directory}")
        
        # Находим файл решения
        sln_path = find_solution_file(directory)
        
        # Парсим решение
        solution = SolutionParser.parse(sln_path)
        
        # Анализируем зависимости
        dependencies = DependencyAnalyzer.analyze(solution, args.with_tests)
        
        # Генерируем диаграмму
        diagram = MermaidGenerator.generate_diagram(dependencies, solution.name)
        
        # Выводим результат
        print(diagram)
        
        logging.debug("Diagram generated successfully")

    except Exception as e:
        logging.exception("Unexpected error occurred")
        sys.exit(1)


if __name__ == "__main__":
    main()