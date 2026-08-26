# -*- coding: utf-8 -*-
"""Проверяет, что галерея показывает все контролы библиотеки.

Галерея — витрина, и цена ей ровно в полноте: контрол, которого в ней нет,
никто не смотрит ни в светлой теме, ни в тёмной. Список руками тут не годится
по той же причине, по какой он не годится в тестах темы, — его забывают.

Запуск из папки галереи:
    python check-coverage.py

Печатает недостающие имена и возвращает 1, если такие есть.
"""
import io, os, re, sys

here = os.path.dirname(os.path.abspath(__file__))
src = os.path.normpath(os.path.join(here, '..', '..', 'src'))

# Что показывает галерея: теги XAML и то, что собирается в code-behind —
# диалог, меню и попап живут не в разметке, а в обработчиках.
shown = set()
for name in ('GalleryCards.axaml', 'GalleryCards.axaml.cs', 'MainWindow.axaml', 'MainWindow.axaml.cs'):
    text = io.open(os.path.join(here, name), encoding='utf-8').read()
    shown |= set(re.findall(r'<ax:(Ax[A-Za-z]+)', text))
    shown |= set(re.findall(r'new (Ax[A-Za-z]+)', text))

# Что есть в библиотеке. Конвертеры не контролы и витрины не требуют.
declared = []
for name in sorted(os.listdir(src)):
    if not name.endswith('.cs'):
        continue
    text = io.open(os.path.join(src, name), encoding='utf-8').read()
    for m in re.finditer(
            r'public\s+(?:sealed\s+)?(?:abstract\s+)?(?:partial\s+)?class\s+(Ax[A-Za-z]+)\s*:\s*([A-Za-z<>, .]+)',
            text):
        if 'IValueConverter' not in m.group(2):
            declared.append(m.group(1))

missing = [name for name in declared if name not in shown]

print('контролов:', len(declared), '· в галерее:', len(declared) - len(missing))
if missing:
    print('нет в галерее:')
    for name in missing:
        print('  ', name)
    sys.exit(1)

print('галерея показывает все')
