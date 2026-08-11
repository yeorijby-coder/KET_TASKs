# -*- coding: utf-8 -*-
# 마크다운 -> Word 가 잘 읽는 HTML.
# pandoc / python-docx 가 없어서 HTML 로 만든 뒤 Word COM 으로 .docx 로 저장한다.
import io
import os
import re
import sys

sys.stdout.reconfigure(encoding='utf-8')

CSS = """
body { font-family:"\ub9d1\uc740 \uace0\ub515","Malgun Gothic",sans-serif; font-size:10.5pt; line-height:1.5; color:#1a1a1a; }
h1 { font-size:20pt; border-bottom:2pt solid #2B57AB; padding-bottom:6pt; color:#14306a; }
h2 { font-size:15pt; margin-top:22pt; border-bottom:1pt solid #b8c6e2; padding-bottom:3pt; color:#1b3f86; }
h3 { font-size:12.5pt; margin-top:16pt; color:#1b3f86; }
h4 { font-size:11pt; margin-top:12pt; color:#33517f; }
p  { margin:5pt 0; }
ul,ol { margin:5pt 0 5pt 20pt; }
li { margin:2pt 0; }
code { font-family:Consolas,"D2Coding",monospace; font-size:9.5pt; background:#f0f2f6; padding:0 2pt; }
pre { font-family:Consolas,"D2Coding",monospace; font-size:9pt; background:#f5f6f9;
      border:0.5pt solid #ccd3e0; padding:6pt; line-height:1.35; }
pre code { background:none; padding:0; font-size:9pt; }
table { border-collapse:collapse; width:100%; margin:6pt 0; font-size:9pt; }
th,td { border:0.5pt solid #99a5bd; padding:3pt 5pt; vertical-align:top; }
th { background:#dfe6f3; font-weight:bold; text-align:left; }
hr { border:0; border-top:0.5pt solid #b8c6e2; }
"""


def esc(s):
    return s.replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;')


def inline(s):
    """백틱 안은 자리표시자로 빼 두고 나머지만 처리한다.
    이렇게 해야 **`코드`** 처럼 굵게가 백틱을 걸쳐 있어도 잡힌다."""
    holds = []

    def keep(m):
        holds.append('<code>' + esc(m.group(1)) + '</code>')
        return '\x00%d\x00' % (len(holds) - 1)

    s = re.sub(r'`([^`]*)`', keep, s)
    s = esc(s)
    s = re.sub(r'\*\*(.+?)\*\*', r'<b>\1</b>', s)
    s = re.sub(r'\[([^\]]+)\]\(([^)]+)\)', r'<a href="\2">\1</a>', s)
    s = s.replace('&lt;br&gt;', '<br/>')     # 표 칸 안 줄바꿈
    s = re.sub('\x00(\\d+)\x00', lambda m: holds[int(m.group(1))], s)
    return s


def is_table_sep(line):
    return bool(re.match(r'^\s*\|?[\s:\-|]+\|[\s:\-|]*$', line)) and '-' in line


def convert(md):
    lines = md.split('\n')
    out = []
    i = 0
    n = len(lines)
    while i < n:
        ln = lines[i]

        # 코드 블록
        if ln.lstrip().startswith('```'):
            i += 1
            buf = []
            while i < n and not lines[i].lstrip().startswith('```'):
                buf.append(lines[i])
                i += 1
            i += 1
            out.append('<pre><code>' + esc('\n'.join(buf)) + '</code></pre>')
            continue

        # 표
        if '|' in ln and i + 1 < n and is_table_sep(lines[i + 1]):
            def cells(row):
                r = row.strip()
                if r.startswith('|'):
                    r = r[1:]
                if r.endswith('|'):
                    r = r[:-1]
                return [c.strip() for c in r.split('|')]

            head = cells(ln)
            i += 2
            body = []
            while i < n and '|' in lines[i] and lines[i].strip():
                body.append(cells(lines[i]))
                i += 1
            t = ['<table><tr>']
            t += ['<th>' + inline(c) + '</th>' for c in head]
            t.append('</tr>')
            for row in body:
                t.append('<tr>' + ''.join('<td>' + inline(c) + '</td>' for c in row) + '</tr>')
            t.append('</table>')
            out.append(''.join(t))
            continue

        # 제목
        m = re.match(r'^(#{1,6})\s+(.*)$', ln)
        if m:
            lv = len(m.group(1))
            out.append('<h%d>%s</h%d>' % (lv, inline(m.group(2)), lv))
            i += 1
            continue

        # 가로줄
        if re.match(r'^\s*(-{3,}|\*{3,})\s*$', ln):
            out.append('<hr/>')
            i += 1
            continue

        # 목록
        if re.match(r'^\s*([-*+]|\d+\.)\s+', ln):
            ordered = bool(re.match(r'^\s*\d+\.\s+', ln))
            items = []
            while i < n and re.match(r'^\s*([-*+]|\d+\.)\s+', lines[i]):
                txt = re.sub(r'^\s*([-*+]|\d+\.)\s+', '', lines[i])
                i += 1
                # 들여쓴 이어붙임 줄
                while i < n and lines[i].strip() and not re.match(r'^\s*([-*+]|\d+\.)\s+', lines[i]) \
                        and lines[i].startswith('  ') and not lines[i].lstrip().startswith('```'):
                    txt += ' ' + lines[i].strip()
                    i += 1
                items.append('<li>' + inline(txt) + '</li>')
            tag = 'ol' if ordered else 'ul'
            out.append('<%s>%s</%s>' % (tag, ''.join(items), tag))
            continue

        # 빈 줄
        if not ln.strip():
            i += 1
            continue

        # 문단 - 다음 빈 줄까지 모은다
        buf = [ln]
        i += 1
        while i < n and lines[i].strip() and not re.match(r'^(#{1,6}\s|\s*```|\s*([-*+]|\d+\.)\s)', lines[i]) \
                and not ('|' in lines[i] and i + 1 < n and is_table_sep(lines[i + 1])):
            buf.append(lines[i])
            i += 1
        out.append('<p>' + '<br/>'.join(inline(b) for b in buf) + '</p>')

    return '\n'.join(out)


def main(src, dst, title):
    md = io.open(src, encoding='utf-8').read()
    html = ('<html><head><meta charset="utf-8"/>'
            '<title>' + esc(title) + '</title>'
            '<style>' + CSS + '</style></head><body>\n' + convert(md) + '\n</body></html>')
    io.open(dst, 'w', encoding='utf-8-sig').write(html)
    print('  ->', dst, len(html), 'bytes')


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else
         os.path.splitext(os.path.basename(sys.argv[1]))[0])
